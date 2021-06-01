using System.Collections.Generic;
using Examine.Lucene.Sync;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Sync
{
    [TestFixture]
    public class SyncLocalTests : ExamineBaseTest
    {
        [Test]
        public void GivenAMainIndex_WhenReplicatedLocally_TheLocalIndexIsPopulated()
        {
            var tempStorage = new System.IO.DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            var indexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());

            using (var mainDir = new RandomIdRAMDirectory())
            using (var localDir = new RandomIdRAMDirectory())
            using (TestIndex mainIndex = GetTestIndex(mainDir, new StandardAnalyzer(LuceneInfo.CurrentVersion), indexDeletionPolicy: indexDeletionPolicy))
            using (var replicator = new ExamineReplicator(
                mainIndex.IndexWriter.IndexWriter,
                localDir,
                tempStorage))
            {
                mainIndex.CreateIndex();

                mainIndex.IndexItems(mainIndex.AllData());

                DirectoryReader mainReader = mainIndex.IndexWriter.IndexWriter.GetReader(true);
                Assert.AreEqual(100, mainReader.NumDocs);

                // TODO: Ok so replication CANNOT occur on an open index with an open IndexWriter.
                // See this note: https://lucenenet.apache.org/docs/4.8.0-beta00014/api/replicator/Lucene.Net.Replicator.IndexReplicationHandler.html
                // "NOTE: This handler assumes that Lucene.Net.Index.IndexWriter is not opened by another process on the index directory. In fact, opening an Lucene.Net.Index.IndexWriter on the same directory to which files are copied can lead to undefined behavior, where some or all the files will be deleted, override other files or simply create a mess. When you replicate an index, it is best if the index is never modified by Lucene.Net.Index.IndexWriter, except the one that is open on the source index, from which you replicate."
                // So if we want to replicate, we can sync from Main on startup and ensure that the writer isn't opened until that
                // is done (the callback can be used for that).
                // If we want to sync back to main, it means we can never open a writer to main, but that might be ok and we
                // publish on a schedule.
                replicator.ReplicateIndex();

                using (TestIndex localIndex = GetTestIndex(localDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
                {
                    DirectoryReader localReader = localIndex.IndexWriter.IndexWriter.GetReader(true);
                    Assert.AreEqual(100, localReader.NumDocs);
                }
            }
        }

        [Test]
        public void GivenASyncedLocalIndex_WhenTriggered_ThenSyncedBackToMainIndex()
        {
            var tempStorage = new System.IO.DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            var indexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());

            using (var mainDir = new RandomIdRAMDirectory())
            using (var localDir = new RandomIdRAMDirectory())
            {
                using (TestIndex mainIndex = GetTestIndex(mainDir, new StandardAnalyzer(LuceneInfo.CurrentVersion), indexDeletionPolicy: indexDeletionPolicy))
                using (var replicator = new ExamineReplicator(
                    mainIndex.IndexWriter.IndexWriter,
                    localDir,
                    tempStorage))
                {
                    mainIndex.CreateIndex();
                    mainIndex.IndexItems(mainIndex.AllData());
                    replicator.ReplicateIndex();
                }

                using (TestIndex localIndex = GetTestIndex(localDir, new StandardAnalyzer(LuceneInfo.CurrentVersion), indexDeletionPolicy: indexDeletionPolicy))
                {
                    localIndex.IndexItem(new ValueSet(9999.ToString(), "content",
                            new Dictionary<string, IEnumerable<object>>
                            {
                                {"item1", new List<object>(new[] {"value1"})},
                                {"item2", new List<object>(new[] {"value2"})}
                            }));

                    using (var replicator = new ExamineReplicator(
                        localIndex.IndexWriter.IndexWriter,
                        mainDir,
                        tempStorage))
                    {
                        // replicate back to main
                        replicator.ReplicateIndex();
                    }

                    using (TestIndex mainIndex = GetTestIndex(mainDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
                    {
                        DirectoryReader mainReader = mainIndex.IndexWriter.IndexWriter.GetReader(true);
                        Assert.AreEqual(101, mainReader.NumDocs);
                    }
                }
            }

        }
    }
}
