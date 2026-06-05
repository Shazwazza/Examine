using System;
using System.IO;
using System.Threading;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Lucene.Net.Replicator;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene
{
    /// <summary>
    /// Used to replicate an index to a destination directory
    /// </summary>
    /// <remarks>
    /// The destination directory must not have any active writers open to it.
    /// </remarks>
    public class ExamineReplicator : IDisposable
    {
        private const string TaxonomyWriterInitializationFailureMessage = "Taxonomy replication is enabled but the taxonomy writer could not be initialized.";
        private const int DefaultMaxConsecutiveReplicationFailures = 5;
        private bool _disposedValue;
        private readonly LocalReplicator _replicator;
        private readonly LuceneIndex _sourceIndex;
        private readonly Directory _sourceDirectory;
        private readonly Directory _destinationDirectory;
        private readonly Directory? _destinationTaxonomyDirectory;
        private readonly Lazy<LoggingReplicationClient> _localReplicationClient;
        private readonly object _locker = new object();
        private bool _started = false;
        private int _consecutiveFailures;
        private readonly ILogger<ExamineReplicator> _logger;
        private readonly bool _taxonomyEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExamineReplicator"/> class.
        /// </summary>
        /// <param name="replicatorLogger">The logger for the replicator.</param>
        /// <param name="clientLogger">The logger for the replication client.</param>
        /// <param name="sourceIndex">The source index to replicate from.</param>
        /// <param name="sourceDirectory">The source directory of the index.</param>
        /// <param name="destinationDirectory">The destination directory for replication.</param>
        /// <param name="destinationTaxonomyDirectory">The destination taxonomy directory for replication. Can be null if taxonomy is disabled.</param>
        /// <param name="tempStorage">The temporary storage directory used during replication.</param>
        public ExamineReplicator(
            ILogger<ExamineReplicator> replicatorLogger,
            ILogger<LoggingReplicationClient> clientLogger,
            LuceneIndex sourceIndex,
            Directory sourceDirectory,
            Directory destinationDirectory,
            Directory? destinationTaxonomyDirectory,
            DirectoryInfo tempStorage)
        {
            _sourceIndex = sourceIndex;
            _sourceDirectory = sourceDirectory;
            _destinationDirectory = destinationDirectory;
            _destinationTaxonomyDirectory = destinationTaxonomyDirectory;
            _taxonomyEnabled = destinationTaxonomyDirectory != null;
            _replicator = new LocalReplicator();
            _logger = replicatorLogger;

            _localReplicationClient = new Lazy<LoggingReplicationClient>(() =>
            {
                IReplicationHandler handler;
                if (_taxonomyEnabled && destinationTaxonomyDirectory != null)
                {
                    handler = new IndexAndTaxonomyReplicationHandler(
                        destinationDirectory,
                        destinationTaxonomyDirectory,
                        () =>
                        {
                            // Callback, can be used to notify when replication is done (i.e. to open the index)
                            if (_logger.IsEnabled(LogLevel.Debug))
                            {
                                var sourceDir = UnwrapDirectory(sourceDirectory);
                                var destDir = UnwrapDirectory(destinationDirectory);
                                var sourceTaxonomyDir = sourceIndex.GetLuceneTaxonomyDirectory() as FSDirectory;
                                var destTaxonomyDir = destinationTaxonomyDirectory as FSDirectory;

                                _logger.LogDebug(
                                    "{IndexName} replication complete from {SourceDirectory} to {DestinationDirectory} and Taxonomy {TaxonomySourceDirectory} to {TaxonomyDestinationDirectory}",
                                    sourceIndex.Name,
                                    sourceDir?.Directory.ToString() ?? "InMemory",
                                    destDir?.Directory.ToString() ?? "InMemory",
                                    sourceTaxonomyDir?.Directory.ToString() ?? "InMemory",
                                    destTaxonomyDir?.Directory.ToString() ?? "InMemory"
                                );
                            }
                        });
                }
                else
                {
                    handler = new IndexReplicationHandler(
                        destinationDirectory,
                        () =>
                        {
                            // Callback, can be used to notify when replication is done (i.e. to open the index)
                            if (_logger.IsEnabled(LogLevel.Debug))
                            {
                                var sourceDir = UnwrapDirectory(sourceDirectory);
                                var destDir = UnwrapDirectory(destinationDirectory);

                                _logger.LogDebug(
                                    "{IndexName} replication complete from {SourceDirectory} to {DestinationDirectory}",
                                    sourceIndex.Name,
                                    sourceDir?.Directory.ToString() ?? "InMemory",
                                    destDir?.Directory.ToString() ?? "InMemory"
                                );
                            }
                        });
                }

                return new LoggingReplicationClient(
                    clientLogger,
                    _replicator,
                    handler,
                    new PerSessionDirectoryFactory(tempStorage.FullName));
            });
        }

        /// <summary>
        /// The number of consecutive scheduled replication failures that are tolerated before replication is
        /// automatically stopped and reported as unhealthy via <see cref="IsReplicationHealthy"/>.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>5</c>. Set to a value of <c>0</c> or less to never stop replication automatically.
        /// </remarks>
        public int MaxConsecutiveReplicationFailures { get; internal set; } = DefaultMaxConsecutiveReplicationFailures;

        /// <summary>
        /// The number of consecutive times scheduled replication has failed to publish a revision.
        /// </summary>
        /// <remarks>
        /// This is reset to zero after a successful publish or when scheduled replication is (re)started.
        /// </remarks>
        public int ConsecutiveReplicationFailures => Volatile.Read(ref _consecutiveFailures);

        /// <summary>
        /// Returns <c>false</c> once scheduled replication has failed <see cref="MaxConsecutiveReplicationFailures"/>
        /// consecutive times and has therefore been stopped, allowing callers to monitor replication health.
        /// </summary>
        public bool IsReplicationHealthy =>
            MaxConsecutiveReplicationFailures <= 0 || Volatile.Read(ref _consecutiveFailures) < MaxConsecutiveReplicationFailures;

        /// <summary>
        /// Will sync from the active index to the destination directory
        /// </summary>
        public void ReplicateIndex()
        {
            if (IndexWriter.IsLocked(_destinationDirectory))
            {
                throw new InvalidOperationException("The destination directory is locked");
            }

            if (_taxonomyEnabled && _destinationTaxonomyDirectory != null && IndexWriter.IsLocked(_destinationTaxonomyDirectory))
            {
                throw new InvalidOperationException("The destination taxonomy directory is locked");
            }

            _logger.LogInformation(
                "Replicating index from {SourceIndex} to {DestinationIndex}",
                _sourceDirectory,
                _destinationDirectory);

            IRevision rev;
            try
            {
                rev = CreateRevision();
            }
            catch (InvalidOperationException ex) when (ex.Message != TaxonomyWriterInitializationFailureMessage)
            {
                // will occur if there is nothing to sync
                _logger.LogInformation("There was nothing to replicate to {DestinationIndex}", _destinationDirectory);
                return;
            }

            _replicator.Publish(rev);
            _localReplicationClient.Value.UpdateNow();

            _logger.LogInformation(
                "Replication from index {SourceIndex} to {DestinationIndex} complete.",
                _sourceDirectory,
                _destinationDirectory);
        }
        
        /// <summary>
        /// Creates a revision based on whether taxonomy is enabled
        /// </summary>
        private IRevision CreateRevision()
        {
            if (_taxonomyEnabled)
            {
                var taxonomyWriterFactory = _sourceIndex.SnapshotDirectoryTaxonomyIndexWriterFactory;
                if (taxonomyWriterFactory?.IndexWriter == null)
                {
                    // Ensure the taxonomy writer has been initialized before attempting to create a taxonomy revision.
                    _ = _sourceIndex.TaxonomyWriter;
                    taxonomyWriterFactory = _sourceIndex.SnapshotDirectoryTaxonomyIndexWriterFactory;
                }

                if (taxonomyWriterFactory?.IndexWriter != null)
                {
                    return new IndexAndTaxonomyRevision(_sourceIndex.IndexWriter.IndexWriter, taxonomyWriterFactory);
                }

                throw new InvalidOperationException(TaxonomyWriterInitializationFailureMessage);
            }

            return new IndexRevision(_sourceIndex.IndexWriter.IndexWriter);
        }

        /// <summary>
        /// Starts index replication
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void StartIndexReplicationOnSchedule(int milliseconds)
        {
            if (_started)
            {
                return;
            }

            lock (_locker)
            {
                if (_started)
                {
                    return;
                }

                if (_sourceIndex.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    if (IndexWriter.IsLocked(_destinationDirectory))
                    {
                        throw new InvalidOperationException("The destination directory is locked");
                    }

                    _sourceIndex.IndexCommitted += SourceIndex_IndexCommitted;

                    // this will update the destination every second if there are changes.
                    // the change monitor will be stopped when this is disposed.
                    _localReplicationClient.Value.StartUpdateThread(milliseconds, $"IndexRep{_sourceIndex.Name}");

                    // Reset any previous failure state now that replication has (re)started successfully.
                    Volatile.Write(ref _consecutiveFailures, 0);
                    _started = true;
                }
                catch (Exception ex)
                {
                    _sourceIndex.IndexCommitted -= SourceIndex_IndexCommitted;
                    _logger.LogError(ex, "Failed to start replication schedule for {IndexName}", _sourceIndex.Name);
                }
            }

        }

        /// <summary>
        /// Whenever the index is committed, publish the new revision to be synced.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SourceIndex_IndexCommitted(object? sender, EventArgs e)
        {
            var index = (LuceneIndex?)sender;
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                if(index == null)
                {
                    _logger.LogWarning("Index is null in {method}", nameof(ExamineReplicator.SourceIndex_IndexCommitted));
                }
                _logger.LogDebug("{IndexName} committed", index?.Name ?? $"({nameof(index)} is null)");
            }

            if (!_sourceIndex.IsCancellationRequested)
            {
                try
                {
                    var rev = CreateRevision();
                    _replicator.Publish(rev);

                    // Successful publish, reset the consecutive failure counter.
                    Volatile.Write(ref _consecutiveFailures, 0);
                }
                catch (Exception ex)
                {
                    var failures = Interlocked.Increment(ref _consecutiveFailures);
                    _logger.LogError(
                        ex,
                        "Failed to publish replication revision for {IndexName} (consecutive failure {FailureCount} of {MaxFailures})",
                        _sourceIndex.Name,
                        failures,
                        MaxConsecutiveReplicationFailures);

                    if (MaxConsecutiveReplicationFailures > 0 && failures >= MaxConsecutiveReplicationFailures)
                    {
                        // Persistent (non-transient) failure. Stop reacting to commits so the failing operation is
                        // not retried indefinitely and surface the condition via IsReplicationHealthy so operators
                        // can detect that replication has stopped working.
                        _sourceIndex.IndexCommitted -= SourceIndex_IndexCommitted;

                        // Reset _started under the lock so a future call to StartIndexReplicationOnSchedule can
                        // re-enter the startup path and restart replication once the underlying issue is resolved.
                        lock (_locker)
                        {
                            _started = false;
                        }

                        _logger.LogCritical(
                            ex,
                            "Replication for {IndexName} has failed {FailureCount} consecutive times and has been stopped. The destination index will no longer be updated until replication is restarted",
                            _sourceIndex.Name,
                            failures);
                    }
                }
            }
        }

        /// <summary>
        /// Disposes the instance
        /// </summary>
        /// <param name="disposing">If the call is coming from Dispose</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _sourceIndex.IndexCommitted -= SourceIndex_IndexCommitted;

                    // Disposal in this order based on lucene.net tests:
                    // https://github.com/apache/lucenenet/blob/6b161d961a7764f2d2dbe90ee2ae03f73ccce019/src/Lucene.Net.Tests.Replicator/IndexReplicationClientTest.cs#L169
                    // replicator client
                    // writer
                    // replicator
                    // publish directory
                    // handler directory

                    // We have:
                    //   writer - done with LuceneIndex
                    //   SyncedFileSystemDirectory - done with LuceneIndex
                    //   - ExamineReplicator (this)
                    //   -- client
                    //   --- replicator
                    //   - publish directory
                    //   - handler directory - done with base class FilterDirectory
                    if (_localReplicationClient.IsValueCreated)
                    {
                        _localReplicationClient.Value.Dispose();
                    }
                    _replicator.Dispose();
                }

                _disposedValue = true;
            }
        }

        /// <inheritdoc />
        public void Dispose() =>
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);

        private static FSDirectory? UnwrapSourceDirectory(Directory dir)
        {
            if (dir is SyncedFileSystemDirectory syncedDir)
            {
                return UnwrapDirectory(syncedDir.LocalLuceneDirectory);
            }

            return UnwrapDirectory(dir);
        }

        private static FSDirectory? UnwrapDirectory(Directory dir)
        {
            if (dir is FSDirectory fsDir)
            {
                return fsDir;
            }

            if (dir is NRTCachingDirectory nrtDir)
            {
                return UnwrapSourceDirectory(nrtDir.Delegate);
            }

            return null;
        }
    }
}
