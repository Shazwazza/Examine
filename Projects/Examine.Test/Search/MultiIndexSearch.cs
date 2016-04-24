using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;
using System.IO;
using Examine.Session;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using NUnit.Framework;

using Version = Lucene.Net.Util.Version;

namespace Examine.Test.Search
{
    //ignored because when run in tandem with other tests it thread locks... dunno why yet.
    [TestFixture, Ignore]
    public class MultiIndexSearch
    {
        [TearDown]
        public void Teardown()
        {
            DisposableCollector.Clean();
        }

        [Test]
        public void MultiIndex_Simple_Search()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            using (var luceneDir1 = new RAMDirectory())
            using (var luceneDir2 = new RAMDirectory())
            using (var luceneDir3 = new RAMDirectory())
            using (var luceneDir4 = new RAMDirectory())
            using (var indexer1 = new TestIndexer(luceneDir1, analyzer))
            using (var indexer2 = new TestIndexer(luceneDir2, analyzer))
            using (var indexer3 = new TestIndexer(luceneDir3, analyzer))
            using (var indexer4 = new TestIndexer(luceneDir4, analyzer))
            using (var session = new ThreadScopedIndexSession(indexer1.SearcherContext, indexer2.SearcherContext, indexer3.SearcherContext, indexer4.SearcherContext))

            {
                indexer1.IndexItem(new ValueSet(1, "content", new { item1 = "value1", item2 = "The agitated zebras gallop back and forth in short, panicky dashes, then skitter off into the absolute darkness." }));
                indexer2.IndexItem(new ValueSet(1, "content", new { item1 = "value2", item2 = "The festival lasts five days and celebrates the victory of good over evil, light over darkness, and knowledge over ignorance." }));
                indexer3.IndexItem(new ValueSet(1, "content", new { item1 = "value3", item2 = "They are expected to confront the darkness and show evidence that they have done so in their papers" }));
                indexer4.IndexItem(new ValueSet(1, "content", new { item1 = "value4", item2 = "Scientists believe the lake could be home to cold-loving microbial life adapted to living in total darkness." }));

                indexer3.IndexItem(new ValueSet(2, "content", new { item1 = "value3", item2 = "Scotch scotch scotch, i love scotch" }));
                indexer4.IndexItem(new ValueSet(2, "content", new { item1 = "value4", item2 = "60% of the time, it works everytime" }));

                session.WaitForChanges();

                var searcher = new MultiIndexSearcher(new[]
                {
                    luceneDir1, luceneDir2, luceneDir3, luceneDir4
                }, analyzer);

                var result = searcher.Search("darkness", false);

                Assert.AreEqual(4, result.TotalItemCount);
            }
        }

        [Test]
        public void MultiIndex_Field_Count()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            using (var luceneDir1 = new RAMDirectory())
            using (var luceneDir2 = new RAMDirectory())
            using (var luceneDir3 = new RAMDirectory())
            using (var luceneDir4 = new RAMDirectory())
            using (var indexer1 = new TestIndexer(luceneDir1, analyzer))
            using (var indexer2 = new TestIndexer(luceneDir2, analyzer))
            using (var indexer3 = new TestIndexer(luceneDir3, analyzer))
            using (var indexer4 = new TestIndexer(luceneDir4, analyzer))
            using (var session = new ThreadScopedIndexSession(indexer1.SearcherContext, indexer2.SearcherContext, indexer3.SearcherContext, indexer4.SearcherContext))

            {
                indexer1.IndexItem(new ValueSet(1, "content", new { item1 = "hello", item2 = "The agitated zebras gallop back and forth in short, panicky dashes, then skitter off into the absolute darkness." }));
                indexer2.IndexItem(new ValueSet(1, "content", new { item1 = "world", item2 = "The festival lasts five days and celebrates the victory of good over evil, light over darkness, and knowledge over ignorance." }));
                indexer3.IndexItem(new ValueSet(1, "content", new { item1 = "here", item2 = "They are expected to confront the darkness and show evidence that they have done so in their papers" }));
                indexer4.IndexItem(new ValueSet(1, "content", new { item1 = "are", item2 = "Scientists believe the lake could be home to cold-loving microbial life adapted to living in total darkness." }));

                indexer3.IndexItem(new ValueSet(2, "content", new { item3 = "some", item2 = "Scotch scotch scotch, i love scotch" }));
                indexer4.IndexItem(new ValueSet(2, "content", new { item4 = "values", item2 = "60% of the time, it works everytime" }));

                session.WaitForChanges();

                var searcher = new MultiIndexSearcher(new[]
                {
                    luceneDir1, luceneDir2, luceneDir3, luceneDir4
                }, analyzer);

                var result = searcher.GetSearchFields();
                //will be item1 , item2, item3, and item4
                Assert.AreEqual(4, result.Count());
                foreach (var s in new[] { "item1", "item2", "item3", "item4" })
                {
                    Assert.IsTrue(result.Contains(s));
                }
            }
        }
    }
}