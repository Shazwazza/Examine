using System;
using System.IO;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Lucene.Net.Replicator;
using Lucene.Net.Store;
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
        private readonly IReplicator _replicator;
        private readonly LuceneIndex _sourceIndex;
        private readonly Directory _destinationDirectory;
        private readonly ReplicationClient _localReplicationClient;
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
        public ExamineReplicator(
            ILoggerFactory loggerFactory,
            LuceneIndex sourceIndex,
            Directory destinationDirectory,
            DirectoryInfo tempStorage)
        {
            _sourceIndex = sourceIndex;
            _destinationDirectory = destinationDirectory;
            _replicator = new LocalReplicator();
            _logger = loggerFactory.CreateLogger<ExamineReplicator>();

            _localReplicationClient = new LoggingReplicationClient(
                loggerFactory.CreateLogger<LoggingReplicationClient>(),
                _replicator,
                new IndexReplicationHandler(
                    destinationDirectory,
                    () =>
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            var sourceDir = sourceIndex.GetLuceneDirectory() as FSDirectory;
                            var destDir = destinationDirectory as FSDirectory;

                            // Callback, can be used to notifiy when replication is done (i.e. to open the index)
                            if (_logger.IsEnabled(LogLevel.Debug))
                            {
                                _logger.LogDebug(
                                    "{IndexName} replication complete from {SourceDirectory} to {DestinationDirectory}",
                                    sourceIndex.Name,
                                    sourceDir?.Directory.ToString() ?? "InMemory",
                                    destDir?.Directory.ToString() ?? "InMemory");
                            }
                        }
                  
                    }),
                new PerSessionDirectoryFactory(tempStorage.FullName));
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
                _sourceIndex.GetLuceneDirectory(),
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
            _localReplicationClient.UpdateNow();

            _logger.LogInformation(
                "Replication from index {SourceIndex} to {DestinationIndex} complete.",
                _sourceIndex.GetLuceneDirectory(),
                _destinationDirectory);
        }

        /// <summary>
        /// Starts index replication
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void StartIndexReplicationOnSchedule(int milliseconds)
        {
            lock (_locker)
            {
                if (_started)
                {
                    return;
                }

                _started = true;

                if (IndexWriter.IsLocked(_destinationDirectory))
                {
                    throw new InvalidOperationException("The destination directory is locked");
                }

                _sourceIndex.IndexCommitted += SourceIndex_IndexCommitted;

                // this will update the destination every second if there are changes.
                // the change monitor will be stopped when this is disposed.
                _localReplicationClient.StartUpdateThread(milliseconds, $"IndexRep{_sourceIndex.Name}");
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
            var rev = new IndexRevision(_sourceIndex.IndexWriter.IndexWriter);
            _replicator.Publish(rev);
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
                    _localReplicationClient.Dispose();
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the instance
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
#pragma warning disable IDE0022 // Use expression body for method
            Dispose(disposing: true);
#pragma warning restore IDE0022 // Use expression body for method
        }
    }
}
