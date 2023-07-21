
using System;
using System.IO;
using System.Threading;
using Examine.Lucene.Providers;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    /// <summary>
    /// A directory factory that replicates the index from main storage on initialization to another
    /// directory, then creates a lucene Directory based on that replicated index. A replication thread
    /// is spawned to then replicate the local index back to the main storage location.
    /// </summary>
    /// <remarks>
    /// By default, Examine configures the local directory to be the %temp% folder.
    /// </remarks>
    public class SyncedFileSystemDirectoryFactory : FileSystemDirectoryFactory
    {
        private readonly DirectoryInfo _localDir;
        private readonly ILoggerFactory _loggerFactory;
        private ExamineReplicator? _replicator;

        /// <inheritdoc/>
        public SyncedFileSystemDirectoryFactory(
            DirectoryInfo localDir,
            DirectoryInfo mainDir,
            ILockFactory lockFactory,
            ILoggerFactory loggerFactory)
            : base(mainDir, lockFactory)
        {
            _localDir = localDir;
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc/>
        protected override Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
        {
            var path = Path.Combine(_localDir.FullName, luceneIndex.Name);
            var localLuceneIndexFolder = new DirectoryInfo(path);

            Directory mainDir = base.CreateDirectory(luceneIndex, forceUnlock);

            // used by the replicator, will be a short lived directory for each synced revision and deleted when finished.
            var tempDir = new DirectoryInfo(Path.Combine(_localDir.FullName, "Rep", Guid.NewGuid().ToString("N")));

            if (DirectoryReader.IndexExists(mainDir))
            {
                // when the lucene directory is going to be created, we'll sync from main storage to local
                // storage before any index/writer is opened.
                using (var tempMainIndexWriter = new IndexWriter(
                    mainDir,
                    new IndexWriterConfig(
                        LuceneInfo.CurrentVersion,
                        new StandardAnalyzer(LuceneInfo.CurrentVersion))
                    {
                        OpenMode = OpenMode.APPEND,
                        IndexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy())
                    }))
                using (var tempMainIndex = new LuceneIndex(_loggerFactory, luceneIndex.Name, new TempOptions(), tempMainIndexWriter))
                using (var tempLocalDirectory = new SimpleFSDirectory(localLuceneIndexFolder, LockFactory.GetLockFactory(localLuceneIndexFolder)))
                using (var replicator = new ExamineReplicator(_loggerFactory, tempMainIndex, tempLocalDirectory, tempDir))
                {
                    if (forceUnlock)
                    {
                        IndexWriter.Unlock(tempLocalDirectory);
                    }

                    // replicate locally.
                    replicator.ReplicateIndex();
                }
            }

            // now create the replicator that will copy from local to main on schedule
            _replicator = new ExamineReplicator(_loggerFactory, luceneIndex, mainDir, tempDir);
            var localLuceneDir = FSDirectory.Open(
                localLuceneIndexFolder,
                LockFactory.GetLockFactory(localLuceneIndexFolder));

            if (forceUnlock)
            {
                IndexWriter.Unlock(localLuceneDir);
            }

            // Start replicating back to main
            _replicator.StartIndexReplicationOnSchedule(1000);

            return localLuceneDir;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _replicator?.Dispose();
            }
        }

        private class TempOptions : IOptionsMonitor<LuceneDirectoryIndexOptions>
        {
            public LuceneDirectoryIndexOptions CurrentValue => new LuceneDirectoryIndexOptions();
            public LuceneDirectoryIndexOptions Get(string name) => CurrentValue;

            public IDisposable OnChange(Action<LuceneDirectoryIndexOptions, string> listener) => throw new NotImplementedException();
        }

    }
}
