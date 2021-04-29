using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.Util;
using NUnit.Framework;

namespace Examine.Test.Search
{
    [TestFixture]
    public class LuceneSearchResultsReaderTrackerTests
    {
        [Test]
        public void Track_Readers()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))
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

                LuceneSearcher searcher = (LuceneSearcher)indexer.GetSearcher();
                IndexSearcher luceneSearcher = (IndexSearcher)searcher.GetLuceneSearcher();

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
