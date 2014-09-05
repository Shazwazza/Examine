using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Examine.Session;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using Lucene.Net.Util;
using NUnit.Framework;

namespace Examine.Test.Search
{
    /// <summary>
    /// Tests specific to Lucene criteria
    /// </summary>
    [TestFixture, RequiresSTA]
    public class LuceneSearchTests
    {
        [TearDown]
        public void Teardown()
        {
            DisposableCollector.Clean();
        }

        //TODO: Write tests for all 'LuceneSearch', 'LuceneQuery', 'Facets*', 'Wrap*' methods

        [Test]
        public void Can_Get_Lucene_Search_Result()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new
                        {
                            nodeName = "my name 1"
                        })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateCriteria("content");
                var filter = criteria.Field("nodeName", "name");
                var results = searcher.Find(filter.Compile());

                Assert.AreEqual(typeof(LuceneSearchResults), results.GetType());
            }
        }

        [Test]
        public void Can_Count_Facets()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { Content = "hello world", Type = "type1" }),
                    new ValueSet(2, "content",
                        new { Content = "hello something or other", Type = "type1" }),
                    new ValueSet(2, "content",
                        new { Content = "hello you guys", Type = "type1" }),
                    new ValueSet(3, "content",
                        new { Content = "hello you cruel world", Type = "type2" }),
                    new ValueSet(4, "content",
                        new { Content = "hi there, hello world", Type = "type2" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateCriteria();
                var filter = criteria
                    .CountFacets(true)
                    .Field("Content", "hello");
                
                var results = searcher.Find(filter.Compile());

                
            }
        }



    }
}