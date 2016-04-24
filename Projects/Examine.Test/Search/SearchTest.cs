using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;
using Examine.Session;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using NUnit.Framework;
using Version = Lucene.Net.Util.Version;

namespace Examine.Test.Search
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestFixture]
	public class SearchTest
    {

        [TearDown]
        public void Teardown()
        {
            DisposableCollector.Clean();
        }

        [Test]
        public void Search_All_Fields_No_Wildcards()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))

            {
                indexer.RebuildIndex();
                session.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);
                var result = searcher.Search("value1", false);

                Assert.AreEqual(1, result.TotalItemCount);
            }    
        }

        [Test]
        public void Search_All_Fields_Wildcards()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))

            {
                indexer.RebuildIndex();
                session.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);
                var result = searcher.Search("value", true);

                Assert.AreEqual(100, result.TotalItemCount);
            }
        }

        [Test]
        public void Search_On_Stop_Word_No_Result()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))

            {
                indexer.IndexItem(new ValueSet(1, "content",
                   new { item1 = "value1", item2 = "here we go into the darkness" }));

                session.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);
                var result = searcher.Search("into", false);

                Assert.AreEqual(0, result.TotalItemCount);
            }
        }

    }
}
