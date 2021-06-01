using System;
using System.IO;
using Lucene.Net.Index;
using Lucene.Net.Replicator;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Sync
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
        private readonly IndexWriter _indexWriter;
        private readonly Directory _destinationDirectory;
        private readonly ReplicationClient _localReplicationClient;

        public ExamineReplicator(
            IndexWriter indexWriter,
            Directory destinationDirectory,
            DirectoryInfo tempStorage)
        {
            _indexWriter = indexWriter;
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

            var rev = new IndexRevision(_indexWriter);
            _replicator.Publish(rev);
            _localReplicationClient.UpdateNow();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
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
