using System;
using System.IO;
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
        private bool _disposedValue;
        private readonly LocalReplicator _replicator;
        private readonly LuceneIndex _sourceIndex;
        private readonly Directory _sourceDirectory;
        private readonly Directory _destinationDirectory;
        private readonly Lazy<LoggingReplicationClient> _localReplicationClient;
        private readonly object _locker = new object();
        private bool _started = false;
        private readonly ILogger<ExamineReplicator> _logger;

        /// <summary>
        /// Creates an instance of <see cref="ExamineReplicator"/>
        /// </summary>
        /// <param name="loggerFactory">The logger factory</param>
        /// <param name="sourceIndex">The source index</param>
        /// <param name="destinationDirectory">The destination directory</param>
        /// <param name="tempStorage">The temp storage directory info</param>
        [Obsolete("Use ctor specifying loggers instead")]
        public ExamineReplicator(
            ILoggerFactory loggerFactory,
            LuceneIndex sourceIndex,
            Directory destinationDirectory,
            DirectoryInfo tempStorage)
            : this(
                  loggerFactory.CreateLogger<ExamineReplicator>(),
                  loggerFactory.CreateLogger<LoggingReplicationClient>(),
                  sourceIndex,
                  destinationDirectory,
                  tempStorage)
        {
        }

        [Obsolete("Use ctor specifying source directory instead")]
        public ExamineReplicator(
            ILogger<ExamineReplicator> replicatorLogger,
            ILogger<LoggingReplicationClient> clientLogger,
            LuceneIndex sourceIndex,
            Directory destinationDirectory,
            DirectoryInfo tempStorage)
            : this(
                  replicatorLogger,
                  clientLogger,
                  sourceIndex,
                  UnwrapSourceDirectory(sourceIndex.GetLuceneDirectory()),
                  destinationDirectory,
                  tempStorage)
        {
        }

        public ExamineReplicator(
            ILogger<ExamineReplicator> replicatorLogger,
            ILogger<LoggingReplicationClient> clientLogger,
            LuceneIndex sourceIndex,
            Directory sourceDirectory,
            Directory destinationDirectory,
            DirectoryInfo tempStorage)
        {
            _sourceIndex = sourceIndex;
            _sourceDirectory = sourceDirectory;
            _destinationDirectory = destinationDirectory;
            _replicator = new LocalReplicator();
            _logger = replicatorLogger;

            _localReplicationClient = new Lazy<LoggingReplicationClient>(()
                => new LoggingReplicationClient(
                    clientLogger,
                    _replicator,
                    new IndexReplicationHandler(
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
                                    destDir?.Directory.ToString() ?? "InMemory");
                            }
                        }),
                    new PerSessionDirectoryFactory(tempStorage.FullName)));
        }

        /// <summary>
        /// Will sync from the active index to the destination directory
        /// </summary>
        public void ReplicateIndex()
        {
            if (IndexWriter.IsLocked(_destinationDirectory))
            {
                throw new InvalidOperationException("The destination directory is locked");
            }

            _logger.LogInformation(
                "Replicating index from {SourceIndex} to {DestinationIndex}",
                _sourceDirectory,
                _destinationDirectory);

            IndexRevision rev;
            try
            {
                rev = new IndexRevision(_sourceIndex.IndexWriter.IndexWriter);
            }
            catch (InvalidOperationException)
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
                _started = true;

                if (_sourceIndex.IsCancellationRequested)
                {
                    return;
                }

                if (IndexWriter.IsLocked(_destinationDirectory))
                {
                    throw new InvalidOperationException("The destination directory is locked");
                }

                _sourceIndex.IndexCommitted += SourceIndex_IndexCommitted;

                // this will update the destination every second if there are changes.
                // the change monitor will be stopped when this is disposed.
                _localReplicationClient.Value.StartUpdateThread(milliseconds, $"IndexRep{_sourceIndex.Name}");
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
                var rev = new IndexRevision(_sourceIndex.IndexWriter.IndexWriter);
                _replicator.Publish(rev);
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

        public void Dispose() =>
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);

        private static FSDirectory UnwrapSourceDirectory(Directory dir)
        {
            if (dir is SyncedFileSystemDirectory syncedDir)
            {
                return UnwrapDirectory(syncedDir.LocalLuceneDirectory);
            }

            return UnwrapDirectory(dir);
        }

        private static FSDirectory UnwrapDirectory(Directory dir)
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
