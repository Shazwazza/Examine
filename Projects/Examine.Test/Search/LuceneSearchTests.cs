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

        //TODO: Finish these

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
                            nodeName = "my name 1",
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Home"
                        }),
                    new ValueSet(2, "content",
                        new
                        {
                            nodeName = "About us",
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Test"
                        }),
                    new ValueSet(3, "content",
                        new
                        {
                            nodeName = "my name 3",
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Page"
                        })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                //paths contain punctuation, we'll escape it and ensure an exact match
                var criteria = searcher.CreateSearchCriteria("content");

                //get all node type aliases starting with CWS_Home OR and all nodees starting with "About"
                var filter = criteria.Field("nodeName", "name");

                var results = searcher.Search<ILuceneSearchResults, LuceneSearchResult>(filter.Compile());

                Assert.AreEqual(typeof(LuceneSearchResults), results.GetType());
            }
        }

    }
}