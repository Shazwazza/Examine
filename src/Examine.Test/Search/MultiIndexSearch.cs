using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Providers;
using System.IO;
using Examine.LuceneEngine;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using NUnit.Framework;
using Version = Lucene.Net.Util.Version;

namespace Examine.Test.Search
{

    [TestFixture]
    public class MultiIndexSearch
    {
        [Test]
        public void MultiIndex_Simple_Search()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            using (var luceneDir1 = new RandomIdRAMDirectory())
            using (var luceneDir2 = new RandomIdRAMDirectory())
            using (var luceneDir3 = new RandomIdRAMDirectory())
            using (var luceneDir4 = new RandomIdRAMDirectory())
            using (var indexer1 = new TestIndex(luceneDir1, analyzer))
            using (var indexer2 = new TestIndex(luceneDir2, analyzer))
            using (var indexer3 = new TestIndex(luceneDir3, analyzer))
            using (var indexer4 = new TestIndex(luceneDir4, analyzer))

            {
                indexer1.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "value1", item2 = "The agitated zebras gallop back and forth in short, panicky dashes, then skitter off into the absolute darkness." }));
                indexer2.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "value2", item2 = "The festival lasts five days and celebrates the victory of good over evil, light over darkness, and knowledge over ignorance." }));
                indexer3.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "value3", item2 = "They are expected to confront the darkness and show evidence that they have done so in their papers" }));
                indexer4.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "value4", item2 = "Scientists believe the lake could be home to cold-loving microbial life adapted to living in total darkness." }));
                indexer3.IndexItem(ValueSet.FromObject(2.ToString(), "content", new { item1 = "value3", item2 = "Scotch scotch scotch, i love scotch" }));
                indexer4.IndexItem(ValueSet.FromObject(2.ToString(), "content", new { item1 = "value4", item2 = "60% of the time, it works everytime" }));

                var searcher = new MultiIndexSearcher("testSearcher",
                    new[] { indexer1.GetSearcher(), indexer2.GetSearcher(), indexer3.GetSearcher(), indexer4.GetSearcher()}.OfType<LuceneSearcher>(), 
                    analyzer);

                var result = searcher.Search("darkness");

                Assert.AreEqual(4, result.TotalItemCount);
                foreach (var r in result)
                {
                    Console.WriteLine("Score = " + r.Score);
                }
            }
        }

        [Test]
        public void MultiIndex_Field_Count()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            using (var luceneDir1 = new RandomIdRAMDirectory())
            using (var luceneDir2 = new RandomIdRAMDirectory())
            using (var luceneDir3 = new RandomIdRAMDirectory())
            using (var luceneDir4 = new RandomIdRAMDirectory())
            using (var indexer1 = new TestIndex(luceneDir1, analyzer) { RunAsync = false })
            using (var indexer2 = new TestIndex(luceneDir2, analyzer) { RunAsync = false })
            using (var indexer3 = new TestIndex(luceneDir3, analyzer) { RunAsync = false })
            using (var indexer4 = new TestIndex(luceneDir4, analyzer) { RunAsync = false })
            {
                indexer1.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "hello", item2 = "The agitated zebras gallop back and forth in short, panicky dashes, then skitter off into the absolute darkness." }));
                indexer2.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "world", item2 = "The festival lasts five days and celebrates the victory of good over evil, light over darkness, and knowledge over ignorance." }));
                indexer3.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "here", item2 = "They are expected to confront the darkness and show evidence that they have done so in their papers" }));
                indexer4.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "are", item2 = "Scientists believe the lake could be home to cold-loving microbial life adapted to living in total darkness." }));
                indexer3.IndexItem(ValueSet.FromObject(2.ToString(), "content", new { item3 = "some", item2 = "Scotch scotch scotch, i love scotch" }));
                indexer4.IndexItem(ValueSet.FromObject(2.ToString(), "content", new { item4 = "values", item2 = "60% of the time, it works everytime" }));
                
                var searcher = new MultiIndexSearcher("testSearcher",
                    new[] { indexer1.GetSearcher(), indexer2.GetSearcher(), indexer3.GetSearcher(), indexer4.GetSearcher()}.OfType<LuceneSearcher>(), 
                    analyzer);

                var result = searcher.GetAllIndexedFields();
                //will be item1 , item2, item3, and item4
                Assert.AreEqual(4, result.Count());
                foreach (var s in new[] { "item1", "item2", "item3", "item4" })
                {
                    Assert.IsTrue(result.Contains(s));
                }
            }
        }

        //[Test]
        //public void MultiIndex_Simple_Search()
        //{
        //    using (var cwsDir = new RandomIdRAMDirectory())
        //    using (var pdfDir = new RandomIdRAMDirectory())
        //    using (var simpleDir = new RandomIdRAMDirectory())
        //    using (var conventionDir = new RandomIdRAMDirectory())
        //    {
        //        //get all of the indexers and rebuild them all first
        //        var indexers = new IIndexer[]
        //                           {
        //                           IndexInitializer.GetUmbracoIndexer(cwsDir),
        //                           IndexInitializer.GetSimpleIndexer(simpleDir),
        //                           IndexInitializer.GetUmbracoIndexer(conventionDir)
        //                           };
        //        foreach (var i in indexers)
        //        {
        //            i.RebuildIndex();
        //        }

        //        using (var cwsIndexer = IndexInitializer.GetUmbracoIndexer(cwsDir))
        //        {
        //            cwsIndexer.RebuildIndex();
        //            //now get the multi index searcher for all indexes
        //            using (var searcher = IndexInitializer.GetMultiSearcher(pdfDir, simpleDir, conventionDir, cwsDir))                
        //            using (var cwsSearcher = IndexInitializer.GetUmbracoSearcher(cwsDir))
        //            {
        //                var cwsResult = cwsSearcher.Search("sam", false);
        //                var result = searcher.Search("sam", false);

        //                //ensure there's more results than just the one index
        //                Assert.IsTrue(cwsResult.Count() < result.Count());
        //                //there should be 8
        //                Assert.AreEqual(8, result.Count(), "Results returned for 'sam' should be equal to 5 with the StandardAnalyzer");
        //            }
        //        };

        //    }



        //}

        //[Test]
        //public void MultiIndex_Field_Count()
        //{
        //    using (var cwsDir = new RandomIdRAMDirectory())
        //    using (var pdfDir = new RandomIdRAMDirectory())
        //    using (var simpleDir = new RandomIdRAMDirectory())
        //    using (var conventionDir = new RandomIdRAMDirectory())
        //    {
        //        //get all of the indexers and rebuild them all first
        //        var indexers = new IIndexer[]
        //                           {
        //                           IndexInitializer.GetUmbracoIndexer(cwsDir),
        //                           IndexInitializer.GetSimpleIndexer(simpleDir),
        //                           IndexInitializer.GetUmbracoIndexer(conventionDir)
        //                           };
        //        foreach (var i in indexers)
        //        {

        //            i.RebuildIndex();
        //        }

        //        //now get the multi index searcher for all indexes
        //        using (var searcher = IndexInitializer.GetMultiSearcher(pdfDir, simpleDir, conventionDir, cwsDir))
        //        {
        //            var result = searcher.GetSearchFields();
        //            Assert.AreEqual(26, result.Count(), "The total number for fields between all of the indexes should be ");
        //        }
        //    }
        //}


    }
}
