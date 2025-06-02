using System.IO;
using Examine.Lucene.Providers;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    internal class SyncedFileSystemDirectory : FilterDirectory
    {
        private readonly ExamineReplicator _replicator;

        public SyncedFileSystemDirectory(
            ILogger<ExamineReplicator> replicatorLogger,
            ILogger<LoggingReplicationClient> clientLogger,
            Directory localLuceneDirectory,
            Directory mainLuceneDirectory,
            LuceneIndex luceneIndex,
            DirectoryInfo tempDir)
            : base(localLuceneDirectory)
        {
            // now create the replicator that will copy from local to main on schedule
            _replicator = new ExamineReplicator(replicatorLogger, clientLogger, luceneIndex, localLuceneDirectory, mainLuceneDirectory, tempDir);
            LocalLuceneDirectory = localLuceneDirectory;
            MainLuceneDirectory = mainLuceneDirectory;
        }

        internal Directory LocalLuceneDirectory { get; }

        internal Directory MainLuceneDirectory { get; }

        public override Lock MakeLock(string name)
        {
            // Start replicating back to main, this is ok to call multiple times, it will only execute once.
            _replicator.StartIndexReplicationOnSchedule(1000);

            return base.MakeLock(name);
        }

        protected override void Dispose(bool disposing)
        {
            _replicator.Dispose();
            MainLuceneDirectory.Dispose();
            base.Dispose(disposing);
        }
    }
}
