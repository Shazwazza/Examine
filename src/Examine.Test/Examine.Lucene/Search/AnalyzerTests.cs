using Examine.Lucene.Analyzers;
using Examine.Lucene.Providers;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Search
{
    [TestFixture]
    public class AnalyzerTests : ExamineBaseTest
    {
        [Test]
        public void Given_CultureInvariantWhitespaceAnalyzer_When_SearchingBothCharVariants_Then_BothAreFound()
        {
            var analyzer = new CultureInvariantWhitespaceAnalyzer();
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { bodyText = "Something rød something"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "Something rod something"})
                });

                var searcher = (BaseLuceneSearcher)indexer.Searcher;

                var query1 = searcher
                    .CreateQuery("content")
                    .Field("bodyText", "rod");
                var results1 = query1.Execute();

                var query2 = searcher
                    .CreateQuery("content")
                    .Field("bodyText", "rød");
                var results2 = query1.Execute();

                Assert.AreEqual(1, results1.TotalItemCount);
            }
        }

        [Test]
        public void Given_CultureInvariantStandardAnalyzer_When_SearchingBothCharVariants_Then_BothAreFound()
        {
            var analyzer = new CultureInvariantStandardAnalyzer();
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { bodyText = "Something rød something"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "Something rod something"})
                });

                var searcher = (BaseLuceneSearcher)indexer.Searcher;

                var query1 = searcher
                    .CreateQuery("content")
                    .Field("bodyText", "rod");
                var results1 = query1.Execute();

                var query2 = searcher
                    .CreateQuery("content")
                    .Field("bodyText", "rød");
                var results2 = query1.Execute();

                Assert.AreEqual(1, results1.TotalItemCount);
            }
        }
    }
}
