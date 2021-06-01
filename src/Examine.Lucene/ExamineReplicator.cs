using System;
using System.IO;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Lucene.Net.Replicator;
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

        public ExamineReplicator(
            LuceneIndex sourceIndex,
            Directory destinationDirectory,
            DirectoryInfo tempStorage)
        {
            _sourceIndex = sourceIndex;
            _destinationDirectory = destinationDirectory;
            _replicator = new LocalReplicator();

            _localReplicationClient = new ReplicationClient(
                _replicator,
                new IndexReplicationHandler(
                    destinationDirectory,
                    // Can be used to notifiy when replication is done (i.e. to open the index)
                    () => true /*on update?*/),
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

            var rev = new IndexRevision(_sourceIndex.IndexWriter.IndexWriter);
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

                _sourceIndex.IndexOperationComplete += Index_IndexOperationComplete;

                // this will update the destination every second if there are changes.
                // the change monitor will be stopped when this is disposed.
                _localReplicationClient.StartUpdateThread(milliseconds, null);
            }

        }

        /// <summary>
        /// Whenever an index operation is complete, publish the new revision to be synced.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Index_IndexOperationComplete(object sender, IndexOperationEventArgs e)
        {
            var rev = new IndexRevision(_sourceIndex.IndexWriter.IndexWriter);
            _replicator.Publish(rev);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _sourceIndex.IndexOperationComplete -= Index_IndexOperationComplete;
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
