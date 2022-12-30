using System;
using System.IO;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Lucene.Net.Replicator;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using static Lucene.Net.Replicator.IndexAndTaxonomyRevision;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene
{
    /// <summary>
    /// Used to replicate an index to a destination directory
    /// </summary>
    /// <remarks>
    /// The destination directory must not have any active writers open to it.
    /// </remarks>
    public class ExamineTaxonomyReplicator : IDisposable
    {
        private bool _disposedValue;
        private readonly IReplicator _replicator;
        private readonly LuceneTaxonomyIndex _sourceIndex;
        private readonly Directory _destinationDirectory;
        private readonly ReplicationClient _localReplicationClient;
        private readonly object _locker = new object();
        private bool _started = false;
        private readonly ILogger<ExamineTaxonomyReplicator> _logger;

        public ExamineTaxonomyReplicator(
            ILoggerFactory loggerFactory,
            LuceneTaxonomyIndex sourceIndex,
            Directory destinationDirectory,
            Directory destinationTaxonomyDirectory,
            DirectoryInfo tempStorage)
        {
            _sourceIndex = sourceIndex;
            _destinationDirectory = destinationDirectory;
            _replicator = new LocalReplicator();
            _logger = loggerFactory.CreateLogger<ExamineTaxonomyReplicator>();

            _localReplicationClient = new LoggingReplicationClient(
                loggerFactory.CreateLogger<LoggingReplicationClient>(),
                _replicator,
                new IndexAndTaxonomyReplicationHandler(
                    destinationDirectory,
                    destinationTaxonomyDirectory,
                    () =>
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            var sourceDir = sourceIndex.GetLuceneDirectory() as FSDirectory;
                            var destDir = destinationDirectory as FSDirectory;


                            var sourceTaxonomyDir = sourceIndex.GetLuceneTaxonomyDirectory() as FSDirectory;
                            var destTaxonomyDir = destinationTaxonomyDirectory as FSDirectory;

                            // Callback, can be used to notifiy when replication is done (i.e. to open the index)
                            if (_logger.IsEnabled(LogLevel.Debug))
                            {
                                _logger.LogDebug(
                                    "{IndexName} replication complete from {SourceDirectory} to {DestinationDirectory} and Taxonomy {TaxonomySourceDirectory} to {TaxonomyDestinationDirectory}",
                                    sourceIndex.Name,
                                    sourceDir?.Directory.ToString() ?? "InMemory",
                                    destDir?.Directory.ToString() ?? "InMemory",
                                    sourceTaxonomyDir?.Directory.ToString() ?? "InMemory",
                                    destTaxonomyDir?.Directory.ToString() ?? "InMemory"
                                    );
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

            IndexAndTaxonomyRevision rev;
            try
            {
                rev = new IndexAndTaxonomyRevision(_sourceIndex.IndexWriter.IndexWriter, _sourceIndex.TaxonomyWriter as SnapshotDirectoryTaxonomyWriter);
            }
            catch (InvalidOperationException)
            {
                // will occur if there is nothing to sync
                return;
            }

            _replicator.Publish(rev);
            _localReplicationClient.UpdateNow();
        }

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
        private void SourceIndex_IndexCommitted(object sender, EventArgs e)
        {
            var index = (LuceneTaxonomyIndex)sender;
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("{IndexName} committed", index.Name);
            }
            var rev = new IndexAndTaxonomyRevision(_sourceIndex.IndexWriter.IndexWriter, _sourceIndex.TaxonomyWriter as SnapshotDirectoryTaxonomyWriter);
            _replicator.Publish(rev);
        }

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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
