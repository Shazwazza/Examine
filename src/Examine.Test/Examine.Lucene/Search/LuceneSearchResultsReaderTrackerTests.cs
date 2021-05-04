using Examine.Lucene.Providers;
using Examine.Lucene.Search;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Search
{
    [TestFixture]
    public class LuceneSearchResultsReaderTrackerTests : ExamineBaseTest
    {
        [Test]
        public void Track_Readers()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "umbraco", headerText = "world", writerName = "administrator" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "umbraco", headerText = "umbraco", writerName = "administrator" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "umbraco", headerText = "umbraco", writerName = "administrator" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "hello", headerText = "world", writerName = "blah" })
                    });

                var searcher = (LuceneSearcher)indexer.GetSearcher();
                IndexReader reader;

                ISearchContext searchContext = searcher.GetSearchContext();
                using (ISearcherReference searchRef = searchContext.GetSearcher())
                {
                    IndexSearcher luceneSearcher = searchRef.IndexSearcher;

                    reader = luceneSearcher.IndexReader;

                    // incremented with call to SearcherManager.Acquire when we get the IndexSearcher from ISearcherReference above
                    // The value starts at 1 when the reader is created.
                    Assert.AreEqual(2, reader.RefCount);

                    //Arrange
                    var sc = searcher.CreateQuery("content").Field("writerName", "administrator");

                    // ensure we are still at 2, the above will have acquired a searcher to read the fields to create the query parser
                    // but will have acquired and released.
                    Assert.AreEqual(2, reader.RefCount);

                    //Act
                    var results = sc.Execute();

                    // we're still at 2, the search has executed and incremented/decremented the counts internally
                    Assert.AreEqual(2, reader.RefCount);
                }

                // back to one, searcher reference is disposed, the SearcherManager has released it.
                Assert.AreEqual(1, reader.RefCount);
            }
        }
    }
}
