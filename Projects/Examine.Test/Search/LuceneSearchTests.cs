using System;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Examine.Session;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using Version = Lucene.Net.Util.Version;

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

        //TODO: Test FieldDefinition.IndexName and figure out what it does!
        //TODO: Write tests for all 'LuceneSearch', 'LuceneQuery', 'Facets*', 'Wrap*' methods

        [Test]
        public void Can_Get_Lucene_Search_Result()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
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
            //TODO: I'm not sure about passing the facet config into the indexer on ctor? 
            // in theory shouldn't we be able to specify this config when we search?

            var config = new FacetConfiguration(new []
            {
                new TermFacetExtractor("manufacturer") ,
                new TermFacetExtractor("resolution")
            });

            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer, config))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { description = "hello world", manufacturer = "Canon", resolution = "2MP" }),
                    new ValueSet(2, "content",
                        new { description = "hello something or other", manufacturer = "Sony", resolution = "4MP" }),
                    new ValueSet(3, "content",
                        new { description = "hello you guys", manufacturer = "Nikon", resolution = "12MP" }),
                    new ValueSet(4, "content",
                        new { description = "hello you cruel world", manufacturer = "Pentax", resolution = "4MP" }),
                    new ValueSet(5, "content",
                        new { description = "hi there, hello world", manufacturer = "Canon", resolution = "12MP" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateCriteria();
                var filter = criteria
                    .CountFacets(true)
                    .Field("description", "hello");
                
                var results = searcher.Find(filter.Compile());

                Assert.AreEqual(2, results.FacetCounts.FacetMap.FieldNames.Count());

                Assert.AreEqual(4, results.FacetCounts.Count(x => x.Key.FieldName == "manufacturer"));

                Assert.AreEqual(2, results.FacetCounts.Single(x => x.Key.Value == "canon").Count);
                Assert.AreEqual(1, results.FacetCounts.Single(x => x.Key.Value == "sony").Count);
                Assert.AreEqual(1, results.FacetCounts.Single(x => x.Key.Value == "pentax").Count);
                Assert.AreEqual(1, results.FacetCounts.Single(x => x.Key.Value == "nikon").Count);

                Assert.AreEqual(3, results.FacetCounts.Count(x => x.Key.FieldName == "resolution"));

                Assert.AreEqual(2, results.FacetCounts.Single(x => x.Key.Value == "4mp").Count);
                Assert.AreEqual(1, results.FacetCounts.Single(x => x.Key.Value == "2mp").Count);
                Assert.AreEqual(2, results.FacetCounts.Single(x => x.Key.Value == "12mp").Count);

                DebutOutputResults(results);
            }
        }

        [Test]
        public void Can_Count_Facets_Refs()
        {
            //TODO: After some investigation, you cannot declare runtime facets, the thing that loads the facets is in ReaderData.ReadFacets which is
            // only called once when ReaderData is ctor'd which is lazily called on the first call to FacetsLoader.GetReaderData

            var config = new FacetConfiguration(new[]
            {
                new TermFacetExtractor("manufacturer"),
                new TermFacetExtractor("resolution"),
                new TermFacetExtractor("similar", true),
                new TermFacetExtractor("recommended", true)
            });

            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer, config))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { description = "hello world", manufacturer = "Canon", resolution = "2MP", similar = 5F, recommended = 5F }),
                    new ValueSet(2, "content",
                        new { description = "hello something or other", manufacturer = "Sony", resolution = "4MP", similar = 4F }),
                    new ValueSet(3, "content",
                        new { description = "hello you guys", manufacturer = "Nikon", resolution = "12MP", similar = 5F }),
                    new ValueSet(4, "content",
                        new { description = "hello you cruel world", manufacturer = "Pentax", resolution = "4MP", similar = 2F }),
                    new ValueSet(5, "content",
                        new []
                        {
                            new KeyValuePair<string, object>("description", "hi there, hello world"),
                            new KeyValuePair<string, object>("manufacturer", "Canon"),
                            new KeyValuePair<string, object>("resolution", "12MP"),
                            new KeyValuePair<string, object>("similar", 1F),
                            new KeyValuePair<string, object>("similar", 3F)
                        }),
                    new ValueSet(6, "content",
                        new { description = "hello, yo, whats up", manufacturer = "Pentax", resolution = "4MP" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateCriteria();
                var filter = criteria
                    .CountFacets(true)
                    .CountFacetReferences(true)
                    .All();

                var results = searcher.Find(filter.Compile());

                DebutOutputResults(results);

                //doc #1 has 1 doc ref by 1 facet
                Assert.AreEqual(1, results.Single(x => x.LongId == 1F).FacetCounts.Count());
                Assert.AreEqual(1, results.Single(x => x.LongId == 1F).FacetCounts.First().Count);
                //doc #2 has 1 doc ref by 1 facet
                Assert.AreEqual(1, results.Single(x => x.LongId == 2F).FacetCounts.Count());
                Assert.AreEqual(1, results.Single(x => x.LongId == 2F).FacetCounts.First().Count);
                //doc #3 has 1 doc ref by 1 facet
                Assert.AreEqual(1, results.Single(x => x.LongId == 3F).FacetCounts.Count());
                Assert.AreEqual(1, results.Single(x => x.LongId == 3F).FacetCounts.First().Count);
                //doc #4 has 1 doc ref by 1 facet
                Assert.AreEqual(1, results.Single(x => x.LongId == 4F).FacetCounts.Count());
                Assert.AreEqual(1, results.Single(x => x.LongId == 4F).FacetCounts.First().Count);
                //doc #5 has 3 doc ref by 2 facet
                Assert.AreEqual(2, results.Single(x => x.LongId == 5F).FacetCounts.Count());
                Assert.AreEqual(2, results.Single(x => x.LongId == 5F).FacetCounts.Single(x => x.FieldName == "similar").Count);
                Assert.AreEqual(1, results.Single(x => x.LongId == 5F).FacetCounts.Single(x => x.FieldName == "recommended").Count);
                //doc #6 has 0 doc ref
                Assert.AreEqual(0, results.Single(x => x.LongId == 6F).FacetCounts.Count());

                

            }
        }


        [Test]
        public void Search_By_Facet_Filter()
        {
            var config = new FacetConfiguration(new[]
            {
                new TermFacetExtractor("manufacturer"),
                new TermFacetExtractor("resolution"),
                new TermFacetExtractor("similar", true),
                new TermFacetExtractor("recommended", true)
            });

            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer, config))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { description = "hello world", manufacturer = "Canon", resolution = "2MP", similar = 5F, recommended = 5F }),
                    new ValueSet(2, "content",
                        new { description = "hello something or other", manufacturer = "Sony", resolution = "4MP", similar = 4F }),
                    new ValueSet(3, "content",
                        new { description = "hello you guys", manufacturer = "Nikon", resolution = "12MP", similar = 5F }),
                    new ValueSet(4, "content",
                        new { description = "hello you cruel world", manufacturer = "Pentax", resolution = "4MP", similar = 2F }),
                    new ValueSet(5, "content",
                        new[]
                        {
                            new KeyValuePair<string, object>("description", "hi there, hello world"),
                            new KeyValuePair<string, object>("manufacturer", "Canon"),
                            new KeyValuePair<string, object>("resolution", "12MP"),
                            new KeyValuePair<string, object>("similar", 1F),
                            new KeyValuePair<string, object>("similar", 3F)
                        }),
                    new ValueSet(6, "content",
                        new { description = "hello, yo, whats up", manufacturer = "Pentax", resolution = "4MP" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateCriteria();
                var filter = criteria
                    .CountFacets(true)
                    .CountFacetReferences(true)
                    .Facets(new FacetKey("manufacturer", "canon"));

                var results = searcher.Find(filter.Compile());

                DebutOutputResults(results);

                Assert.AreEqual(2, results.TotalItemCount);

                Assert.AreEqual(4, results.FacetCounts.FacetMap.FieldNames.Count());

                Assert.AreEqual(4, results.FacetCounts.Count(x => x.Key.FieldName == "manufacturer"));

                Assert.AreEqual(2, results.FacetCounts.Single(x => x.Key.Value == "canon").Count);
                Assert.AreEqual(0, results.FacetCounts.Single(x => x.Key.Value == "sony").Count);
                Assert.AreEqual(0, results.FacetCounts.Single(x => x.Key.Value == "pentax").Count);
                Assert.AreEqual(0, results.FacetCounts.Single(x => x.Key.Value == "nikon").Count);

                Assert.AreEqual(3, results.FacetCounts.Count(x => x.Key.FieldName == "resolution"));

                Assert.AreEqual(0, results.FacetCounts.Single(x => x.Key.Value == "4mp").Count);
                Assert.AreEqual(1, results.FacetCounts.Single(x => x.Key.Value == "2mp").Count);
                Assert.AreEqual(1, results.FacetCounts.Single(x => x.Key.Value == "12mp").Count);
            }
        }


        [Test]
        public void Facet_Count_Is_Null_When_Disabled()
        {
            var config = new FacetConfiguration(new[]
            {
                new TermFacetExtractor("manufacturer"),
                new TermFacetExtractor("resolution")
            });

            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer, config))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { description = "hello world", manufacturer = "Canon", resolution = "2MP" }),
                    new ValueSet(2, "content",
                        new { description = "hello something or other", manufacturer = "Sony", resolution = "4MP" }),
                    new ValueSet(3, "content",
                        new { description = "hello you guys", manufacturer = "Nikon", resolution = "12MP" }),
                    new ValueSet(4, "content",
                        new { description = "hello you cruel world", manufacturer = "Pentax", resolution = "4MP" }),
                    new ValueSet(5, "content",
                        new { description = "hi there, hello world", manufacturer = "Canon", resolution = "12MP" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateCriteria();
                var filter = criteria
                    .CountFacets(false)
                    .Field("description", "hello");

                var results = searcher.Find(filter.Compile());

                Assert.IsNull(results.FacetCounts);
            }
        }

        [Test]
        public void Facet_Count_On_Result_Is_Null_When_Disabled()
        {
            var config = new FacetConfiguration(new[]
            {
                new TermFacetExtractor("manufacturer"),
                new TermFacetExtractor("resolution")
            });

            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer, config))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { description = "hello world", manufacturer = "Canon", resolution = "2MP" }),
                    new ValueSet(2, "content",
                        new { description = "hello something or other", manufacturer = "Sony", resolution = "4MP" }),
                    new ValueSet(3, "content",
                        new { description = "hello you guys", manufacturer = "Nikon", resolution = "12MP" }),
                    new ValueSet(4, "content",
                        new { description = "hello you cruel world", manufacturer = "Pentax", resolution = "4MP" }),
                    new ValueSet(5, "content",
                        new { description = "hi there, hello world", manufacturer = "Canon", resolution = "12MP" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateCriteria();
                var filter = criteria
                    //NOTE: This is false by default!
                    .CountFacetReferences(false)
                    .Field("description", "hello");

                var results = searcher.Find(filter.Compile());

                Assert.IsNull(results.First().FacetCounts);
            }
        }

        [Test]
        public void Can_Highlight_Results()
        {
            //TODO: I'm not sure about passing the facet config into the indexer on ctor? 
            // in theory shouldn't we be able to specify this config when we search?

            var config = new FacetConfiguration(new[]
            {
                new TermFacetExtractor("manufacturer"),
                new TermFacetExtractor("resolution")
            });

            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer, config))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { description = "hello world", manufacturer = "Canon", resolution = "2MP" }),
                    new ValueSet(2, "content",
                        new { description = "hello something or other", manufacturer = "Sony", resolution = "4MP" }),
                    new ValueSet(3, "content",
                        new { description = "hello you guys", manufacturer = "Nikon", resolution = "12MP" }),
                    new ValueSet(4, "content",
                        new { description = "hello you cruel world", manufacturer = "Pentax", resolution = "4MP" }),
                    new ValueSet(5, "content",
                        new { description = "hi there, hello world", manufacturer = "Canon", resolution = "12MP" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateCriteria();
                var filter = criteria
                    .CountFacets(true)
                    .Field("description", "hello");

                var results = searcher.Find(filter.Compile());
                
                 //:: RESULT :: <em>hello</em> world
                Assert.IsTrue(results.ElementAt(0).GetHighlight("description").Contains("<span class='search-highlight'>hello</span>"));
                //:: RESULT :: <em>hello</em> something or other
                Assert.IsTrue(results.ElementAt(1).GetHighlight("description").Contains("<span class='search-highlight'>hello</span>"));
                //:: RESULT :: <em>hello</em> you guys
                Assert.IsTrue(results.ElementAt(2).GetHighlight("description").Contains("<span class='search-highlight'>hello</span>"));
                //:: RESULT :: <em>hello</em> you cruel world
                Assert.IsTrue(results.ElementAt(3).GetHighlight("description").Contains("<span class='search-highlight'>hello</span>"));
                //:: RESULT :: hi there, <em>hello</em> world
                Assert.IsTrue(results.ElementAt(4).GetHighlight("description").Contains("<span class='search-highlight'>hello</span>"));
                
                DebutOutputResults(results);
            }
        }

        private void DebutOutputResults(ILuceneSearchResults results)
        {

            if (results.FacetCounts != null)
            {
                Console.WriteLine(" :: FACETS");
                foreach (var fc in results.FacetCounts)
                {
                    Console.WriteLine(fc.Key + " : " + fc.Count);
                }
            }

            foreach (var result in results)
            {

                Console.WriteLine(" :: RESULT :: " + result.GetHighlight("description"));
                if (result.FacetCounts != null)
                {
                    foreach (var fc in result.FacetCounts)
                    {
                        Console.WriteLine(fc.FieldName + " : " + fc.Count);
                    }
                }
            }
        }
    }
}