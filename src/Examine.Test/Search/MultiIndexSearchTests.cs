using System;
using System.Linq;
using Examine.Lucene.Providers;
using Lucene.Net.Analysis.Standard;
using NUnit.Framework;
using Lucene.Net.Analysis.Util;

namespace Examine.Test.Search
{

    [TestFixture]
    public class MultiIndexSearchTests : ExamineBaseTest
    {
        public static CharArraySet StopWords { get; } = new CharArraySet(LuceneInfo.CurrentVersion, new[]
            {
                "a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "into",
                "is", "it", "no", "not", "of", "on", "or", "such", "that", "their", "then",
                "there", "these", "they", "this", "to", "was", "with"
            }, true);

        [Test]
        public void GivenCustomStopWords_WhenUsedOnlyForSearchingAndNotIndexing_TheDefaultWordsWillBeStrippedDuringIndexing()
        {
            var customAnalyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion, StopWords);
            var standardAnalyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);

            using (var luceneDir1 = new RandomIdRAMDirectory())
            using (var luceneDir2 = new RandomIdRAMDirectory())
            // using the StandardAnalyzer on the indexes means that the default stop words
            // will get stripped from the text before being stored in the index.
            using (var indexer1 = GetTestIndex(luceneDir1, standardAnalyzer))
            using (var indexer2 = GetTestIndex(luceneDir2, standardAnalyzer))
            {
                indexer1.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "value1", item2 = "The agitated zebras will gallop back and forth in short, panicky dashes, then skitter off into the absolute darkness." }));
                indexer2.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "value4", item2 = "Scientists believe the lake will be home to cold-loving microbial life adapted to living in total darkness." }));

                using (var searcher = new MultiIndexSearcher("testSearcher",
                    new[] { indexer1, indexer2 },
                    customAnalyzer))
                {
                    // Even though the custom analyzer doesn't have a stop word of 'will'
                    // it will still return nothing because the word has been stripped during indexing.
                    var result = searcher.Search("will");
                    Assert.AreEqual(0, result.TotalItemCount);
                }
            }
        }

        [Test]
        public void GivenCustomStopWords_WhenUsedOnlyForIndexingAndNotForSearching_TheDefaultWordsWillNotBeStrippedDuringSearchingWithManagedQueries()
        {
            var customAnalyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion, StopWords);
            var standardAnalyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);

            using (var luceneDir1 = new RandomIdRAMDirectory())
            using (var luceneDir2 = new RandomIdRAMDirectory())
            // using the CustomAnalyzer on the indexes means that the custom stop words
            // will get stripped from the text before being stored in the index.
            using (var indexer1 = GetTestIndex(luceneDir1, customAnalyzer))
            using (var indexer2 = GetTestIndex(luceneDir2, customAnalyzer))
            {
                indexer1.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "value1", item2 = "The agitated zebras will gallop back and forth in short, panicky dashes, then skitter off into the absolute darkness." }));
                indexer2.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "value4", item2 = "Scientists believe the lake will be home to cold-loving microbial life adapted to living in total darkness." }));

                using (var searcher = new MultiIndexSearcher("testSearcher",
                    new[] { indexer1, indexer2 },
                    // The Analyzer here is used for query parsing values when 
                    // non ManagedQuery queries are executed.
                    standardAnalyzer))
                {
                    // A text search like this will use a ManagedQuery which means it will
                    // use the analyzer assigned to each field to parse the query
                    // which means in this case the passed in StandardAnalyzer is NOT used
                    // and therefor the 'will' word is not stripped and we still get results.
                    var result = searcher.Search("will");
                    Assert.AreEqual(2, result.TotalItemCount);
                }
            }
        }

        [Test]
        public void GivenCustomStopWords_WhenUsedOnlyForIndexingAndNotForSearching_TheDefaultWordsWillBeStrippedDuringSearchingWithNonManagedQueries()
        {
            var customAnalyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion, StopWords);
            var standardAnalyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);

            using (var luceneDir1 = new RandomIdRAMDirectory())
            using (var luceneDir2 = new RandomIdRAMDirectory())
            // using the CustomAnalyzer on the indexes means that the custom stop words
            // will get stripped from the text before being stored in the index.
            using (var indexer1 = GetTestIndex(luceneDir1, customAnalyzer))
            using (var indexer2 = GetTestIndex(luceneDir2, customAnalyzer))
            {
                indexer1.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "value1", item2 = "The agitated zebras will gallop back and forth in short, panicky dashes, then skitter off into the absolute darkness." }));
                indexer2.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "value4", item2 = "Scientists believe the lake will be home to cold-loving microbial life adapted to living in total darkness." }));

                using (var searcher = new MultiIndexSearcher("testSearcher",
                    new[] { indexer1, indexer2 },
                    // The Analyzer here is used for query parsing values when 
                    // non ManagedQuery queries are executed.
                    standardAnalyzer))
                {
                    var result = searcher
                        .CreateQuery("content")
                        // This is a non-ManagedQuery, so the searchers Query Parser
                        // will execute and remove stop words
                        .GroupedOr(new[] { "item1", "item2" }, "will")
                        .Execute();

                    Assert.AreEqual(0, result.TotalItemCount);
                }
            }
        }

        [Test]
        public void Dont_Initialize_Searchers_On_Dispose_If_Not_Already_Initialized()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);

            using (var luceneDir1 = new RandomIdRAMDirectory())
            using (var luceneDir2 = new RandomIdRAMDirectory())
            using (var luceneDir3 = new RandomIdRAMDirectory())
            using (var luceneDir4 = new RandomIdRAMDirectory())
            using (var indexer1 = GetTestIndex(luceneDir1, analyzer))
            using (var indexer2 = GetTestIndex(luceneDir2, analyzer))
            using (var indexer3 = GetTestIndex(luceneDir3, analyzer))
            using (var indexer4 = GetTestIndex(luceneDir4, analyzer))
            {
                var searcher = new MultiIndexSearcher("testSearcher",
                    new[] { indexer1, indexer2, indexer3, indexer4 },
                    analyzer);

                Assert.IsFalse(searcher.SearchersInitialized);

                searcher.Dispose();

                Assert.IsFalse(searcher.SearchersInitialized);
            }
        }

        [Test]
        public void MultiIndex_Simple_Search()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);

            using (var luceneDir1 = new RandomIdRAMDirectory())
            using (var luceneDir2 = new RandomIdRAMDirectory())
            using (var luceneDir3 = new RandomIdRAMDirectory())
            using (var luceneDir4 = new RandomIdRAMDirectory())
            using (var indexer1 = GetTestIndex(luceneDir1, analyzer))
            using (var indexer2 = GetTestIndex(luceneDir2, analyzer))
            using (var indexer3 = GetTestIndex(luceneDir3, analyzer))
            using (var indexer4 = GetTestIndex(luceneDir4, analyzer))

            {
                indexer1.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "value1", item2 = "The agitated zebras gallop back and forth in short, panicky dashes, then skitter off into the absolute darkness." }));
                indexer2.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "value2", item2 = "The festival lasts five days and celebrates the victory of good over evil, light over darkness, and knowledge over ignorance." }));
                indexer3.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "value3", item2 = "They are expected to confront the darkness and show evidence that they have done so in their papers" }));
                indexer4.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "value4", item2 = "Scientists believe the lake could be home to cold-loving microbial life adapted to living in total darkness." }));
                indexer3.IndexItem(ValueSet.FromObject(2.ToString(), "content", new { item1 = "value3", item2 = "Scotch scotch scotch, i love scotch" }));
                indexer4.IndexItem(ValueSet.FromObject(2.ToString(), "content", new { item1 = "value4", item2 = "60% of the time, it works everytime" }));

                using (var searcher = new MultiIndexSearcher("testSearcher",
                    new[] { indexer1, indexer2, indexer3, indexer4 },
                    analyzer))
                {
                    var result = searcher.Search("darkness");

                    Assert.AreEqual(4, result.TotalItemCount);
                    foreach (var r in result)
                    {
                        Console.WriteLine("Score = " + r.Score);
                    }
                }
            }
        }

        [Test]
        public void MultiIndex_Field_Count()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);

            using (var luceneDir1 = new RandomIdRAMDirectory())
            using (var luceneDir2 = new RandomIdRAMDirectory())
            using (var luceneDir3 = new RandomIdRAMDirectory())
            using (var luceneDir4 = new RandomIdRAMDirectory())
            using (var indexer1 = GetTestIndex(luceneDir1, analyzer))            
            using (var indexer2 = GetTestIndex(luceneDir2, analyzer))
            using (var indexer3 = GetTestIndex(luceneDir3, analyzer))
            using (var indexer4 = GetTestIndex(luceneDir4, analyzer))
            using (indexer1.ProcessNonAsync())
            using (indexer2.ProcessNonAsync())
            using (indexer3.ProcessNonAsync())
            using (indexer4.ProcessNonAsync())
            {
                indexer1.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "hello", item2 = "The agitated zebras gallop back and forth in short, panicky dashes, then skitter off into the absolute darkness." }));
                indexer2.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "world", item2 = "The festival lasts five days and celebrates the victory of good over evil, light over darkness, and knowledge over ignorance." }));
                indexer3.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "here", item2 = "They are expected to confront the darkness and show evidence that they have done so in their papers" }));
                indexer4.IndexItem(ValueSet.FromObject(1.ToString(), "content", new { item1 = "are", item2 = "Scientists believe the lake could be home to cold-loving microbial life adapted to living in total darkness." }));
                indexer3.IndexItem(ValueSet.FromObject(2.ToString(), "content", new { item3 = "some", item2 = "Scotch scotch scotch, i love scotch" }));
                indexer4.IndexItem(ValueSet.FromObject(2.ToString(), "content", new { item4 = "values", item2 = "60% of the time, it works everytime" }));

                using (var searcher = new MultiIndexSearcher(
                    "testSearcher",
                    new[] { indexer1, indexer2, indexer3, indexer4 },
                    analyzer))
                {
                    var searchContext = searcher.GetSearchContext();
                    var result = searchContext.SearchableFields;

                    //will be item1 , item2, item3, and item4
                    Assert.AreEqual(4, result.Count());
                    foreach (var s in new[] { "item1", "item2", "item3", "item4" })
                    {
                        Assert.IsTrue(result.Contains(s));
                    }
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
