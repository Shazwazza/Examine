using Examine.Lucene.Providers;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.Util;
using NUnit.Framework;

namespace Examine.Test.Search
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
                IndexSearcher luceneSearcher = searcher.GetLuceneSearcher();

                //Arrange
                var sc = searcher.CreateQuery("content").Field("writerName", "administrator");

                //Act
                var results = sc.Execute();

                using (var e1 = results.GetEnumerator())
                {
                    Assert.AreEqual(2, luceneSearcher.IndexReader.RefCount);
                    using (var e2 = results.Skip(2).GetEnumerator())
                    {
                        Assert.AreEqual(3, luceneSearcher.IndexReader.RefCount);
                    }
                    Assert.AreEqual(2, luceneSearcher.IndexReader.RefCount);
                }
                Assert.AreEqual(1, luceneSearcher.IndexReader.RefCount);
            }
        }
    }
}
