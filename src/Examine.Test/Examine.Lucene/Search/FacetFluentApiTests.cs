using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Lucene;
using Examine.Lucene.Providers;
using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Facet.Range;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using NUnit.Framework;



namespace Examine.Test.Examine.Lucene.Search
{
    [TestFixture]
    public class FacetFluentApiTests : ExamineBaseTest
    {

        /*
         * Below is a copy of the tests from FluentApiTests
         * With faceted search added to ensure that faceting
         * doesn't break the search when used
         */

        [Test]
        public void Allow_Leading_Wildcards_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia"})
                    });

                var searcher = (BaseLuceneSearcher)indexer.Searcher;

                var query1 = searcher.CreateQuery(
                    "content",
                    BooleanOperation.And,
                    searcher.LuceneAnalyzer,
                    new LuceneSearchOptions
                    {
                        AllowLeadingWildcard = true
                    }).NativeQuery("*dney")
                        .And()
                        .Facet("nodeName");

                Assert.Throws<ParseException>(() =>
                    searcher.CreateQuery(
                        "content",
                        BooleanOperation.And,
                        searcher.LuceneAnalyzer,
                        new LuceneSearchOptions
                        {
                            AllowLeadingWildcard = false
                        }).NativeQuery("*dney"));

                var results1 = query1.Execute();

                var facetResults = results1.GetFacet("nodeName");

                Assert.AreEqual(2, results1.TotalItemCount);
                Assert.AreEqual(2, facetResults.Count());
                Assert.AreEqual(1, facetResults.First().Value);
            }
        }

        [Test]
        public void NativeQuery_Single_Word_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer), new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia"})
                    });

                var searcher = indexer.Searcher;

                var query = searcher.CreateQuery("content")
                        .NativeQuery("sydney")
                        .And()
                        .Facet("nodeName");

                Console.WriteLine(query);

                var results = query.Execute();

                var facetResults = results.GetFacet("nodeName");

                Assert.AreEqual(2, results.TotalItemCount);
                Assert.AreEqual(2, facetResults.Count());
                Assert.AreEqual(1, facetResults.Last().Value);
            }
        }

        [Test]
        public void Uppercase_Category_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer), new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "cOntent",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "cOntent",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "cOntent",
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia"})
                    });

                var searcher = indexer.Searcher;

                var query = searcher.CreateQuery("cOntent")
                    .Facet("nodeName")
                    .And()
                    .All();

                Console.WriteLine(query);

                var results = query.Execute();

                var facetResults = results.GetFacet("nodeName");

                Assert.AreEqual(3, results.TotalItemCount);
                Assert.AreEqual(3, facetResults.Count());
            }
        }

        [Test]
        public void NativeQuery_Phrase_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer), new FieldDefinition("bodyText", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "location 3", bodyText = "In Australia there is a town called Bateau Bay in NSW"})
                    });

                var searcher = indexer.Searcher;

                var query = searcher.CreateQuery("content").NativeQuery("\"town called\"").And().Facet("bodyText");

                Console.WriteLine(query);
                Assert.AreEqual("{ Category: content, LuceneQuery: +(nodeName:\"town called\" bodyText:\"town called\") }", query.ToString());

                var results = query.Execute();

                var facetResults = results.GetFacet("bodyText");

                Assert.AreEqual(2, results.TotalItemCount);
                Assert.AreEqual(2, facetResults.Count());
            }
        }

        [Test]
        public void Managed_Range_Date_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("created", FieldDefinitionTypes.FacetDateTime))))
            {


                indexer.IndexItems(new[]
                {
                    ValueSet.FromObject(123.ToString(), "content",
                        new
                        {
                            created = new DateTime(2000, 01, 02),
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Home"
                        }),
                    ValueSet.FromObject(2123.ToString(), "content",
                        new
                        {
                            created = new DateTime(2000, 01, 04),
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Test"
                        }),
                    ValueSet.FromObject(3123.ToString(), "content",
                        new
                        {
                            created = new DateTime(2000, 01, 05),
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Page"
                        })
                });


                var searcher = indexer.Searcher;

                var numberSortedCriteria = searcher.CreateQuery()
                    .RangeQuery<DateTime>(new[] { "created" }, new DateTime(2000, 01, 02), new DateTime(2000, 01, 05))
                    .And()
                    .Facet("created", new Int64Range[]
                    {
                        new Int64Range("First days", new DateTime(2000, 01, 01).Ticks, true, new DateTime(2000, 01, 03).Ticks, true),
                        new Int64Range("Last days", new DateTime(2000, 01, 04).Ticks, true, new DateTime(2000, 01, 06).Ticks, true)
                    });

                var numberSortedResult = numberSortedCriteria.Execute();

                var facetResult = numberSortedResult.GetFacet("created");

                Assert.AreEqual(3, numberSortedResult.TotalItemCount);
                Assert.AreEqual(2, facetResult.Count());
                Assert.AreEqual(1, facetResult.First().Value);
                Assert.AreEqual("First days", facetResult.First().Label);
                Assert.AreEqual(2, facetResult.Last().Value);
                Assert.AreEqual("Last days", facetResult.Last().Label);
            }
        }

        [Test]
        public void Managed_Full_Text_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);

            using (var luceneDir1 = new RandomIdRAMDirectory())
            using (var indexer1 = GetTestIndex(
                luceneDir1,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("item1", FieldDefinitionTypes.FacetFullText))))
            {
                indexer1.IndexItem(ValueSet.FromObject("1", "content", new { item1 = "value1", item2 = "The agitated zebras gallop back and forth in short, panicky dashes, then skitter off into the total absolute darkness." }));
                indexer1.IndexItem(ValueSet.FromObject("2", "content", new { item1 = "value2", item2 = "The festival lasts five days and celebrates the victory of good over evil, light over darkness, and knowledge over ignorance." }));
                indexer1.IndexItem(ValueSet.FromObject("3", "content", new { item1 = "value3", item2 = "They are expected to confront the darkness and show evidence that they have done so in their papers" }));
                indexer1.IndexItem(ValueSet.FromObject("4", "content", new { item1 = "value4", item2 = "Scientists believe the lake could be home to cold-loving microbial life adapted to living in total darkness." }));
                indexer1.IndexItem(ValueSet.FromObject("5", "content", new { item1 = "value3", item2 = "Scotch scotch scotch, i love scotch" }));
                indexer1.IndexItem(ValueSet.FromObject("6", "content", new { item1 = "value4", item2 = "60% of the time, it works everytime" }));
                indexer1.IndexItem(ValueSet.FromObject("7", "content", new { SomeField = "value5", AnotherField = "another value" }));

                var searcher = indexer1.Searcher;

                var result = searcher.CreateQuery()
                    .ManagedQuery("darkness")
                    .And()
                    .Facet("item1")
                    .Execute();

                var facetResults = result.GetFacet("item1");

                Assert.AreEqual(4, result.TotalItemCount);
                Assert.AreEqual(4, facetResults.Count());

                Console.WriteLine("Search 1:");
                foreach (var r in result)
                {
                    Console.WriteLine($"Id = {r.Id}, Score = {r.Score}");
                }

                result = searcher.CreateQuery()
                    .ManagedQuery("total darkness")
                    .And()
                    .Facet("item1")
                    .Execute();
                facetResults = result.GetFacet("item1");

                Assert.AreEqual(2, result.TotalItemCount);
                Assert.AreEqual(2, facetResults.Count());
                Console.WriteLine("Search 2:");
                foreach (var r in result)
                {
                    Console.WriteLine($"Id = {r.Id}, Score = {r.Score}");
                }
            }
        }

        [Test]
        public void Managed_Full_Text_With_Bool_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);

            using (var luceneDir1 = new RandomIdRAMDirectory())
            using (var indexer1 = GetTestIndex(
                luceneDir1,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("item1", FieldDefinitionTypes.FacetFullText))))
            {
                indexer1.IndexItem(ValueSet.FromObject("1", "content", new { item1 = "value1", item2 = "The agitated zebras gallop back and forth in short, panicky dashes, then skitter off into the total absolute darkness." }));
                indexer1.IndexItem(ValueSet.FromObject("2", "content", new { item1 = "value2", item2 = "The festival lasts five days and celebrates the victory of good over evil, light over darkness, and knowledge over ignorance." }));
                indexer1.IndexItem(ValueSet.FromObject("3", "content", new { item1 = "value3", item2 = "They are expected to confront the darkness and show evidence that they have done so in their papers" }));
                indexer1.IndexItem(ValueSet.FromObject("4", "content", new { item1 = "value4", item2 = "Scientists believe the lake could be home to cold-loving microbial life adapted to living in total darkness." }));
                indexer1.IndexItem(ValueSet.FromObject("5", "content", new { item1 = "value3", item2 = "Scotch scotch scotch, i love scotch" }));
                indexer1.IndexItem(ValueSet.FromObject("6", "content", new { item1 = "value4", item2 = "60% of the time, it works everytime" }));

                var searcher = indexer1.Searcher;

                var qry = searcher.CreateQuery().ManagedQuery("darkness").And().Field("item1", "value1").And().Facet("item1");
                Console.WriteLine(qry);
                var result = qry.Execute();

                var facetResults = result.GetFacet("item1");

                Assert.AreEqual(1, result.TotalItemCount);
                Assert.AreEqual(1, facetResults.Count());
                Console.WriteLine("Search 1:");
                foreach (var r in result)
                {
                    Console.WriteLine($"Id = {r.Id}, Score = {r.Score}");
                }

                qry = searcher.CreateQuery().ManagedQuery("darkness")
                    .And(query => query.Field("item1", "value1").Or().Field("item1", "value2"), BooleanOperation.Or)
                    .And()
                    .Facet("item1");
                Console.WriteLine(qry);
                result = qry.Execute();

                facetResults = result.GetFacet("item1");

                Assert.AreEqual(2, result.TotalItemCount);
                Assert.AreEqual(2, facetResults.Count());
                Console.WriteLine("Search 2:");
                foreach (var r in result)
                {
                    Console.WriteLine($"Id = {r.Id}, Score = {r.Score}");
                }
            }
        }

        [Test]
        public void Not_Managed_Full_Text_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);

            using (var luceneDir1 = new RandomIdRAMDirectory())
            using (var indexer1 = GetTestIndex(
                luceneDir1,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("item1", FieldDefinitionTypes.FacetFullText))
                ))
            {
                indexer1.IndexItem(ValueSet.FromObject("1", "content", new { item1 = "value1", item2 = "The agitated zebras gallop back and forth in short, panicky dashes, then skitter off into the total absolute chaos." }));
                indexer1.IndexItem(ValueSet.FromObject("2", "content", new { item1 = "value1", item2 = "The festival lasts five days and celebrates the victory of good over evil, light over darkness, and knowledge over ignorance." }));
                indexer1.IndexItem(ValueSet.FromObject("3", "content", new { item1 = "value3", item2 = "They are expected to confront the darkness and show evidence that they have done so in their papers" }));
                indexer1.IndexItem(ValueSet.FromObject("4", "content", new { item1 = "value4", item2 = "Scientists believe the lake could be home to cold-loving microbial life adapted to living in total darkness." }));
                indexer1.IndexItem(ValueSet.FromObject("5", "content", new { item1 = "value3", item2 = "Scotch scotch scotch, i love scotch" }));
                indexer1.IndexItem(ValueSet.FromObject("6", "content", new { item1 = "value4", item2 = "60% of the time, it works everytime" }));

                var searcher = indexer1.Searcher;

                var qry = searcher.CreateQuery()
                    .Field("item1", "value1")
                    .Not().ManagedQuery("darkness")
                    .And()
                    .Facet("item1");

                Console.WriteLine(qry);
                var result = qry.Execute();

                var facetResults = result.GetFacet("item1");

                Assert.AreEqual(1, result.TotalItemCount);
                Assert.AreEqual("1", result.ElementAt(0).Id);
                Assert.AreEqual(1, facetResults.Count());

                Console.WriteLine("Search 1:");
                foreach (var r in result)
                {
                    Console.WriteLine($"Id = {r.Id}, Score = {r.Score}");
                }
            }
        }

        [Test]
        public void Managed_Range_Int_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.FacetInteger))))
            {


                indexer.IndexItems(new[]
                {
                    ValueSet.FromObject(123.ToString(), "content",
                        new
                        {
                            parentID = 121,
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Home"
                        }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new
                        {
                            parentID = 123,
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Test"
                        }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new
                        {
                            parentID = 124,
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Page"
                        })
                });

                var searcher = indexer.Searcher;

                var numberSortedCriteria = searcher.CreateQuery()
                    .RangeQuery<int>(new[] { "parentID" }, 122, 124)
                    .And()
                    .Facet("parentID", new Int64Range[]
                    {
                        new Int64Range("120-122", 120, true, 122, true),
                        new Int64Range("123-125", 123, true, 125, true)
                    });

                var numberSortedResult = numberSortedCriteria.Execute();

                var facetResults = numberSortedResult.GetFacet("parentID");

                Assert.AreEqual(2, numberSortedResult.TotalItemCount);
                Assert.AreEqual(2, facetResults.Count());
                Assert.AreEqual(0, facetResults.First(result => result.Label == "120-122").Value);
                Assert.AreEqual(2, facetResults.First(result => result.Label == "123-125").Value);
            }
        }

        [Test]
        public void Legacy_ParentId_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.FacetInteger))))
            {


                indexer.IndexItems(new[]
                {
                    ValueSet.FromObject(123.ToString(), "content",
                        new
                        {
                            nodeName = "my name 1",
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Home"
                        }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new
                        {
                            parentID = 123,
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Test"
                        }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new
                        {
                            parentID = 123,
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Page"
                        })
                });

                var searcher = indexer.Searcher;

                var numberSortedCriteria = searcher.CreateQuery()
                    .Field("parentID", 123)
                    .And()
                    .Facet("parentID")
                    .OrderBy(new SortableField("sortOrder", SortType.Int));

                var numberSortedResult = numberSortedCriteria.Execute();

                var facetResults = numberSortedResult.GetFacet("parentID");

                Assert.AreEqual(2, numberSortedResult.TotalItemCount);
            }


        }

        [Test]
        public void Grouped_Or_Examiness_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("nodeTypeAlias", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[]
                {
                    ValueSet.FromObject(1.ToString(), "content",
                        new
                        {
                            nodeName = "my name 1",
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Home"
                        }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new
                        {
                            nodeName = "About us",
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Test"
                        }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new
                        {
                            nodeName = "my name 3",
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Page"
                        })
                });

                var searcher = indexer.Searcher;

                //paths contain punctuation, we'll escape it and ensure an exact match
                var criteria = searcher.CreateQuery("content").Facet("nodeTypeAlias").And();

                //get all node type aliases starting with CWS_Home OR and all nodees starting with "About"
                var filter = criteria.GroupedOr(
                    new[] { "nodeTypeAlias", "nodeName" },
                    new[] { "CWS_Home".Boost(10), "About".MultipleCharacterWildcard() });

                Console.WriteLine(filter);

                var results = filter.Execute();

                var facetResults = results.GetFacet("nodeTypeAlias");

                foreach (var r in results)
                {
                    Console.WriteLine($"Id = {r.Id}");
                }

                Assert.AreEqual(2, results.TotalItemCount);

                Assert.AreEqual(2, facetResults.Count());

            }
        }

        [Test]
        public void Grouped_Or_Query_Output_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer))

            {
                var searcher = indexer.Searcher;

                Console.WriteLine("GROUPED OR - SINGLE FIELD, MULTI VAL");
                var criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedOr(new[] { "id" }.ToList(), new[] { "1", "2", "3" }).And().Facet("SomeFacet");
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(id:1 id:2 id:3)", criteria.Query.ToString());

                Console.WriteLine("GROUPED OR - MULTI FIELD, MULTI VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedOr(new[] { "id", "parentID" }.ToList(), new[] { "1", "2", "3" }).And().Facet("SomeFacet");
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(id:1 id:2 id:3 parentID:1 parentID:2 parentID:3)", criteria.Query.ToString());

                Console.WriteLine("GROUPED OR - MULTI FIELD, EQUAL MULTI VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedOr(new[] { "id", "parentID", "blahID" }.ToList(), new[] { "1", "2", "3" }).And().Facet("SomeFacet");
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(id:1 id:2 id:3 parentID:1 parentID:2 parentID:3 blahID:1 blahID:2 blahID:3)", criteria.Query.ToString());

                Console.WriteLine("GROUPED OR - MULTI FIELD, SINGLE VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedOr(new[] { "id", "parentID" }.ToList(), new[] { "1" }).And().Facet("SomeFacet");
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(id:1 parentID:1)", criteria.Query.ToString());

                Console.WriteLine("GROUPED OR - SINGLE FIELD, SINGLE VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedOr(new[] { "id" }.ToList(), new[] { "1" }).And().Facet("SomeFacet");
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(id:1)", criteria.Query.ToString());

            }


        }

        [Test]
        public void Grouped_And_Query_Output_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer))

            {
                var searcher = indexer.Searcher;
                //new LuceneSearcher("testSearcher", luceneDir, analyzer);

                Console.WriteLine("GROUPED AND - SINGLE FIELD, MULTI VAL");
                var criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedAnd(new[] { "id" }.ToList(), new[] { "1", "2", "3" }).And().Facet("SomeFacet", "SomeValue");
                Console.WriteLine(criteria.Query);
                //We used to assert this, but it must be allowed to do an add on the same field multiple times
                //Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1)", criteria.Query.ToString());
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1 +id:2 +id:3)", criteria.Query.ToString());

                Console.WriteLine("GROUPED AND - MULTI FIELD, EQUAL MULTI VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedAnd(new[] { "id", "parentID", "blahID" }.ToList(), new[] { "1", "2", "3" }).And().Facet("SomeFacet", "SomeValue");
                Console.WriteLine(criteria.Query);
                //The field/value array lengths are equal so we will match the key/value pairs
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1 +parentID:2 +blahID:3)", criteria.Query.ToString());

                Console.WriteLine("GROUPED AND - MULTI FIELD, MULTI VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedAnd(new[] { "id", "parentID" }.ToList(), new[] { "1", "2", "3" }).And().Facet("SomeFacet", "SomeValue");
                Console.WriteLine(criteria.Query);
                //There are more than one field and there are more values than fields, in this case we align the key/value pairs
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1 +parentID:2)", criteria.Query.ToString());

                Console.WriteLine("GROUPED AND - MULTI FIELD, SINGLE VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedAnd(new[] { "id", "parentID" }.ToList(), new[] { "1" }).And().Facet("SomeFacet", "SomeValue");
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1 +parentID:1)", criteria.Query.ToString());

                Console.WriteLine("GROUPED AND - SINGLE FIELD, SINGLE VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedAnd(new[] { "id" }.ToList(), new[] { "1" }).And().Facet("SomeFacet", "SomeValue");
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1)", criteria.Query.ToString());
            }
        }

        /// <summary>
        /// CANNOT BE A MUST WITH NOT i.e. +(-id:1 -id:2 -id:3)  --> That will not work with the "+"
        /// </summary>
        [Test]
        public void Grouped_Not_Query_Output_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer))

            {
                var searcher = indexer.Searcher;

                Console.WriteLine("GROUPED NOT - SINGLE FIELD, MULTI VAL");
                var criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedNot(new[] { "id" }.ToList(), new[] { "1", "2", "3" }).And().Facet("SomeFacet", new Int64Range[] { new Int64Range("Label", 0, true, 120, true) });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias -id:1 -id:2 -id:3", criteria.Query.ToString());

                Console.WriteLine("GROUPED NOT - MULTI FIELD, MULTI VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedNot(new[] { "id", "parentID" }.ToList(), new[] { "1", "2", "3" }).And().Facet("SomeFacet", new Int64Range[] { new Int64Range("Label", 0, true, 120, true) });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias -id:1 -id:2 -id:3 -parentID:1 -parentID:2 -parentID:3", criteria.Query.ToString());

                Console.WriteLine("GROUPED NOT - MULTI FIELD, EQUAL MULTI VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedNot(new[] { "id", "parentID", "blahID" }.ToList(), new[] { "1", "2", "3" }).And().Facet("SomeFacet", new Int64Range[] { new Int64Range("Label", 0, true, 120, true) });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias -id:1 -id:2 -id:3 -parentID:1 -parentID:2 -parentID:3 -blahID:1 -blahID:2 -blahID:3", criteria.Query.ToString());

                Console.WriteLine("GROUPED NOT - MULTI FIELD, SINGLE VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedNot(new[] { "id", "parentID" }.ToList(), new[] { "1" }).And().Facet("SomeFacet", new Int64Range[] { new Int64Range("Label", 0, true, 120, true) });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias -id:1 -parentID:1", criteria.Query.ToString());

                Console.WriteLine("GROUPED NOT - SINGLE FIELD, SINGLE VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedNot(new[] { "id" }.ToList(), new[] { "1" }).And().Facet("SomeFacet", new Int64Range[] { new Int64Range("Label", 0, true, 120, true) });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias -id:1", criteria.Query.ToString());
            }
        }

        [Test]
        public void Grouped_Not_Single_Field_Single_Value_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {

                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", bodyText = "lorem ficus", headerText = "header 1", umbracoNaviHide = "1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "lorem ficus", headerText = "header 2", umbracoNaviHide = "0" })
                    });

                var searcher = indexer.Searcher;

                var query = (LuceneSearchQuery)searcher.CreateQuery("content");
                query.GroupedNot(new[] { "umbracoNaviHide" }, 1.ToString()).And().Facet("nodeName");
                Console.WriteLine(query.Query);
                var results = query.Execute();

                var facetResults = results.GetFacet("nodeName");

                Assert.AreEqual(1, results.TotalItemCount);
                Assert.AreEqual(1, facetResults.Count());
            }
        }

        [Test]
        public void Grouped_Not_Multi_Field_Single_Value_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {

                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", bodyText = "lorem ficus", show = "1", umbracoNaviHide = "1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "lorem ficus", show = "2", umbracoNaviHide = "0" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "my name 3", bodyText = "lorem ficus", show = "1", umbracoNaviHide = "0" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "my name 4", bodyText = "lorem ficus", show = "0", umbracoNaviHide = "1" })
                });

                var searcher = indexer.Searcher;

                var query = searcher.CreateQuery("content").GroupedNot(new[] { "umbracoNaviHide", "show" }, 1.ToString()).And().Facet("nodeName");
                Console.WriteLine(query);
                var results = query.Execute();

                var facetResults = results.GetFacet("nodeName");

                Assert.AreEqual(1, results.TotalItemCount);
                Assert.AreEqual(1, facetResults.Count());
                Assert.AreEqual(1, facetResults.Facet("my name 2").Value);
            }
        }

        [Test]
        public void Grouped_Or_With_Not_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(

                //TODO: Making this a number makes the query fail - i wonder how to make it work correctly?
                // It's because the searching is NOT using a managed search
                //new[] { new FieldDefinition("umbracoNaviHide", FieldDefinitionTypes.Integer) }, 

                luceneDir, analyzer,
                new FieldDefinitionCollection(new FieldDefinition("headerText", FieldDefinitionTypes.FacetFullText))))
            {


                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", bodyText = "lorem ipsum", headerText = "header 1", umbracoNaviHide = "1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", headerText = "header 2", umbracoNaviHide = "0" })
                    });

                var searcher = indexer.Searcher;

                //paths contain punctuation, we'll escape it and ensure an exact match
                var criteria = searcher.CreateQuery("content");
                var filter = criteria.GroupedOr(new[] { "nodeName", "bodyText", "headerText" }, "ipsum").Not().Field("umbracoNaviHide", "1").And().Facet("headerText");
                var results = filter.Execute();

                var facetResults = results.GetFacet("headerText");

                Assert.AreEqual(1, results.TotalItemCount);
                Assert.AreEqual(1, facetResults.Facet("header 2").Value);
            }
        }

        [Test]
        public void And_Grouped_Not_Single_Value_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", bodyText = "lorem ficus", headerText = "header 1", umbracoNaviHide = "1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", headerText = "header 2", umbracoNaviHide = "0" })
                    });

                var searcher = indexer.Searcher;

                var query = searcher.CreateQuery("content")
                    .Field("nodeName", "name")
                    .And().GroupedOr(new[] { "bodyText" }, new[] { "ficus", "ipsum" })
                    .And().GroupedNot(new[] { "umbracoNaviHide" }, new[] { 1.ToString() })
                    .And().Facet("nodeName");

                Console.WriteLine(query);
                var results = query.Execute();

                var facetResults = results.GetFacet("nodeName");

                Assert.AreEqual(1, results.TotalItemCount);
                Assert.AreEqual(1, facetResults.Count());
            }
        }

        [Test]
        public void And_Grouped_Not_Multi_Value_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", bodyText = "lorem ficus", headerText = "header 1", umbracoNaviHide = "1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", headerText = "header 2", umbracoNaviHide = "0" })
                    });

                var searcher = indexer.Searcher;

                var query = searcher.CreateQuery("content")
                    .Field("nodeName", "name")
                    .And().GroupedOr(new[] { "bodyText" }, new[] { "ficus", "ipsum" })
                    .And().GroupedNot(new[] { "umbracoNaviHide" }, new[] { 1.ToString(), 2.ToString() })
                    .And().Facet("nodeName");

                Console.WriteLine(query);
                var results = query.Execute();
                var facetResults = results.GetFacet("nodeName");

                Assert.AreEqual(1, results.TotalItemCount);
                Assert.AreEqual(1, facetResults.Count());
            }
        }

        [Test]
        public void And_Not_Single_Field_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", bodyText = "lorem ficus", headerText = "header 1", umbracoNaviHide = "1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", headerText = "header 2", umbracoNaviHide = "0" })
                    });

                var searcher = indexer.Searcher;

                var query = searcher.CreateQuery("content")
                    .Field("nodeName", "name")
                    .And().GroupedOr(new[] { "bodyText" }, new[] { "ficus", "ipsum" })
                    .Not().Field("umbracoNaviHide", 1.ToString())
                    .And().Facet("nodeName");

                Console.WriteLine(query);
                var results = query.Execute();

                var facetResults = results.GetFacets();

                Assert.AreEqual(1, results.TotalItemCount);
                Assert.AreEqual(1, facetResults.Count());
                Assert.AreEqual(1, facetResults.First().Count());
            }
        }

        [Test]
        public void AndNot_Nested_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", bodyText = "lorem ficus", headerText = "header 1", umbracoNaviHide = "1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", headerText = "header 2", umbracoNaviHide = "0" })
                    });

                var searcher = indexer.Searcher;

                var query = searcher.CreateQuery("content")
                    .Field("nodeName", "name")
                    .And().GroupedOr(new[] { "bodyText" }, new[] { "ficus", "ipsum" })
                    .AndNot(x => x.Field("umbracoNaviHide", 1.ToString()))
                    .And().Facet("nodeName");

                // TODO: This results in { Category: content, LuceneQuery: +nodeName:name +(bodyText:ficus bodyText:ipsum) -(+umbracoNaviHide:1) }
                // Which I don't think is right with the -(+ syntax but it still seems to work.

                Console.WriteLine(query);
                var results = query.Execute();

                var facetResults = results.GetFacet("nodeName");
                Assert.AreEqual(1, results.TotalItemCount);
                Assert.AreEqual(1, facetResults.Count());
            }
        }

        [Test]
        public void And_Not_Added_Later_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", bodyText = "lorem ficus", headerText = "header 1", umbracoNaviHide = "1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", headerText = "header 2", umbracoNaviHide = "0" })
                    });

                var searcher = indexer.Searcher;

                var query = searcher.CreateQuery("content")
                    .Field("nodeName", "name");

                query = query
                    .And().GroupedNot(new[] { "umbracoNaviHide" }, new[] { 1.ToString(), 2.ToString() })
                    .And().Facet("nodeName");

                // Results in { Category: content, LuceneQuery: +nodeName:name -umbracoNaviHide:1 -umbracoNaviHide:2 }

                Console.WriteLine(query);
                var results = query.Execute();

                var facetResults = results.GetFacet("nodeName");
                Assert.AreEqual(1, results.TotalItemCount);
                Assert.AreEqual(1, facetResults.Count());
            }
        }

        [Test]
        public void Not_Range_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("start", FieldDefinitionTypes.FacetInteger))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", bodyText = "lorem ficus", headerText = "header 1", start = 100 }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", headerText = "header 2", start = 200 })
                    });

                var searcher = indexer.Searcher;

                var query = searcher.CreateQuery("content")
                    .Field("nodeName", "name")
                    .Not().Field("start", 200)
                    .And().Facet("start", new Int64Range[] { new Int64Range("Label", 100, false, 200, false) });

                Console.WriteLine(query);
                var results = query.Execute();

                var facetResults = results.GetFacet("start");
                Assert.AreEqual(1, results.TotalItemCount);
                Assert.AreEqual(results.First().Id, 1.ToString());
                Assert.AreEqual(0, facetResults.Facet("Label").Value);
            }
        }

        [Test]
        public void Match_By_Path_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);

            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("__Path", "raw"), new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {


                indexer.IndexItems(new[] {
                    new ValueSet(1.ToString(), "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 1"},
                            {"bodyText", "lorem ipsum"},
                            {"__Path", "-1,123,456,789"}
                        }),
                    new ValueSet(2.ToString(), "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 2"},
                            {"bodyText", "lorem ipsum"},
                            {"__Path", "-1,123,456,987"}
                        })
                    });



                var searcher = indexer.Searcher;

                //paths contain punctuation, we'll escape it and ensure an exact match
                var criteria = searcher.CreateQuery("content");
                var filter = criteria.Field("__Path", "-1,123,456,789").And().Facet("nodeName");
                var results1 = filter.Execute();
                var facetResults1 = results1.GetFacet("nodeName");
                Assert.AreEqual(1, results1.TotalItemCount);
                Assert.AreEqual(1, facetResults1.Count());

                //now escape it
                var exactcriteria = searcher.CreateQuery("content");
                var exactfilter = exactcriteria.Field("__Path", "-1,123,456,789".Escape()).And().Facet("nodeName");
                var results2 = exactfilter.Execute();
                var facetResults2 = results2.GetFacet("nodeName");
                Assert.AreEqual(1, results2.TotalItemCount);
                Assert.AreEqual(1, facetResults2.Count());

                //now try with native
                var nativeCriteria = searcher.CreateQuery();
                var nativeFilter = nativeCriteria.NativeQuery("__Path:\\-1,123,456,789").And().Facet("nodeName");
                Console.WriteLine(nativeFilter);
                var results5 = nativeFilter.Execute();
                var facetResults5 = results5.GetFacet("nodeName");
                Assert.AreEqual(1, results5.TotalItemCount);
                Assert.AreEqual(1, facetResults5.Count());

                //now try wildcards
                var wildcardcriteria = searcher.CreateQuery("content");
                var wildcardfilter = wildcardcriteria.Field("__Path", "-1,123,456,".MultipleCharacterWildcard()).And().Facet("nodeName");
                var results3 = wildcardfilter.Execute();
                var facetResults3 = results3.GetFacet("nodeName");
                Assert.AreEqual(2, results3.TotalItemCount);
                Assert.AreEqual(2, facetResults3.Count());
                //not found
                wildcardcriteria = searcher.CreateQuery("content");
                wildcardfilter = wildcardcriteria.Field("__Path", "-1,123,457,".MultipleCharacterWildcard()).And().Facet("nodeName");
                results3 = wildcardfilter.Execute();
                facetResults3 = results3.GetFacet("nodeName");
                Assert.AreEqual(0, results3.TotalItemCount);
                Assert.AreEqual(0, facetResults3?.Count() ?? 0);
            }


        }

        [Test]
        public void Find_By_ParentId_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer), new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", bodyText = "lorem ipsum", parentID = "1235" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", parentID = "1139" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "my name 3", bodyText = "lorem ipsum", parentID = "1139" })
                    });

                var searcher = indexer.Searcher;

                var criteria = searcher.CreateQuery("content");
                var filter = criteria.Field("parentID", 1139).And().Facet("nodeName");

                var results = filter.Execute();

                var facetResults = results.GetFacet("nodeName");

                Assert.AreEqual(2, results.TotalItemCount);
                Assert.AreEqual(2, facetResults.Count());
            }
        }

        [Test]
        public void Find_By_ParentId_Native_Query_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.FacetInteger))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", bodyText = "lorem ipsum", parentID = "1235" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", parentID = "1139" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "my name 3", bodyText = "lorem ipsum", parentID = "1139" })
                    });

                var searcher = indexer.Searcher;

                var criteria = searcher.CreateQuery("content").Facet("parentID").And();

                //NOTE: This will not work :/ 
                // It seems that this answer is along the lines of why: https://stackoverflow.com/questions/45516870/apache-lucene-6-queryparser-range-query-is-not-working-with-intpoint
                // because the field is numeric, this range query will generate a TermRangeQuery which isn't compatible with numerics and what is annoying
                // is the query parser docs uses a numerical figure as examples: https://lucene.apache.org/core/2_9_4/queryparsersyntax.html#Range%20Searches
                // BUT looking closely, those numeric figures are actually dates stored in a specific way that this will work.
                var filter = criteria.NativeQuery("parentID:[1139 TO 1139]");

                //This thread says we could potentially make this work by overriding the query parser: https://stackoverflow.com/questions/5026185/how-do-i-make-the-queryparser-in-lucene-handle-numeric-ranges

                //We can use a Lucene query directly instead:
                //((LuceneSearchQuery)criteria).LuceneQuery(NumericRangeQuery)

                var results = filter.Execute();

                var facetResults = results.GetFacet("parentID");

                Assert.AreEqual(2, results.TotalItemCount);
                Assert.AreEqual(1, facetResults.Count());
                Assert.AreEqual(2, facetResults.Facet("1139").Value);
            }
        }

        [Test]
        public void Find_By_NodeTypeAlias_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("nodeTypeAlias", "raw"), new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {


                indexer.IndexItems(new[] {
                    new ValueSet(1.ToString(), "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 1"},
                            {"nodeTypeAlias", "CWS_Home"}
                            //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Home"}
                        }),
                    new ValueSet(2.ToString(), "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 2"},
                            {"nodeTypeAlias", "CWS_Home"}
                            //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Home"}
                        }),
                    new ValueSet(3.ToString(), "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 3"},
                            {"nodeTypeAlias", "CWS_Page"}
                            //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Page"}
                        })
                    });



                var searcher = indexer.Searcher;

                var criteria = searcher.CreateQuery("content");
                var filter = criteria.Field("nodeTypeAlias", "CWS_Home".Escape()).And().Facet("nodeName");

                var results = filter.Execute();

                var facetResults = results.GetFacet("nodeName");

                Assert.AreEqual(2, results.TotalItemCount);
                Assert.AreEqual(2, facetResults.Count());
            }
        }

        [Test]
        public void Search_With_Stop_Words_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))


            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "into 1", bodyText = "It was one thing to bring Carmen into it, but Jonathan was another story." }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "Hands shoved backwards into his back pockets, he took slow deliberate steps, as if he had something on his mind." }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "my name 3", bodyText = "Slowly carrying the full cups into the living room, she handed one to Alex." })
                    });

                var searcher = indexer.Searcher;

                var criteria = searcher.CreateQuery();

                // TODO: This isn't testing correctly because the search parser is actually removing stop words to generate the search so we actually
                // end up with an empty search and then by fluke this test passes.

                var filter = criteria.Field("bodyText", "into")
                    .Or().Field("nodeName", "into")
                    .And().Facet("nodeName");

                Console.WriteLine(filter);

                var results = filter.Execute();

                var facetResults = results.GetFacet("nodeName");

                Assert.AreEqual(0, results.TotalItemCount);
                Assert.AreEqual(0, facetResults?.Count() ?? 0);
            }
        }

        [Test]
        public void Search_Native_Query_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeTypeAlias", FieldDefinitionTypes.FacetFullText))))


            {


                indexer.IndexItems(new[] {
                    new ValueSet(1.ToString(), "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 1"},
                            {"nodeTypeAlias", "CWS_Home"}
                            //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Home"}
                        }),
                    new ValueSet(2.ToString(), "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 2"},
                            {"nodeTypeAlias", "CWS_Home"}
                            //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Home"}
                        }),
                    new ValueSet(3.ToString(), "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 3"},
                            {"nodeTypeAlias", "CWS_Page"}
                            //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Page"}
                        })
                    });

                var searcher = indexer.Searcher;

                var criteria = searcher.CreateQuery("content");

                var results = criteria.NativeQuery("nodeTypeAlias:CWS_Home").And().Facet("nodeTypeAlias").Execute();

                var facetResults = results.GetFacet("nodeTypeAlias");

                Assert.AreEqual(2, results.TotalItemCount);
                Assert.AreEqual(1, facetResults.Count());
                Assert.AreEqual(2, facetResults.Facet("CWS_Home").Value);
            }

        }


        [Test]
        public void Find_Only_Image_Media_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeTypeAlias", FieldDefinitionTypes.FacetFullText))))


            {


                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "media",
                        new { nodeName = "my name 1", bodyText = "lorem ipsum", nodeTypeAlias = "image" }),
                    ValueSet.FromObject(2.ToString(), "media",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", nodeTypeAlias = "image" }),
                    ValueSet.FromObject(3.ToString(), "media",
                        new { nodeName = "my name 3", bodyText = "lorem ipsum", nodeTypeAlias = "file" })
                    });

                var searcher = indexer.Searcher;

                var criteria = searcher.CreateQuery("media");
                var filter = criteria.Field("nodeTypeAlias", "image").And().Facet("nodeTypeAlias");

                var results = filter.Execute();

                var facetResults = results.GetFacet("nodeTypeAlias");

                Assert.AreEqual(2, results.TotalItemCount);
                Assert.AreEqual(1, facetResults.Count());
                Assert.AreEqual(2, facetResults.Facet("image").Value);
            }
        }

        [Test]
        public void Find_Both_Media_And_Content_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "media",
                        new { nodeName = "my name 1", bodyText = "lorem ipsum", nodeTypeAlias = "image" }),
                    ValueSet.FromObject(2.ToString(), "media",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", nodeTypeAlias = "image" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "my name 3", bodyText = "lorem ipsum", nodeTypeAlias = "file" }),
                    ValueSet.FromObject(4.ToString(), "other",
                        new { nodeName = "my name 4", bodyText = "lorem ipsum", nodeTypeAlias = "file" })
                    });

                var searcher = indexer.Searcher;

                var criteria = searcher.CreateQuery(defaultOperation: BooleanOperation.Or);
                var filter = criteria
                    .Field(ExamineFieldNames.CategoryFieldName, "media")
                    .Or()
                    .Field(ExamineFieldNames.CategoryFieldName, "content")
                    .And()
                    .Facet("nodeName");

                var results = filter.Execute();

                var facetResults = results.GetFacet("nodeName");

                Assert.AreEqual(3, results.TotalItemCount);
                Assert.AreEqual(3, facetResults.Count());
            }
        }

        [Test]
        public void Sort_Result_By_Number_Field_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                //Ensure it's set to a number, otherwise it's not sortable
                new FieldDefinitionCollection(new FieldDefinition("sortOrder", FieldDefinitionTypes.FacetInteger), new FieldDefinition("parentID", FieldDefinitionTypes.FacetInteger))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", sortOrder = "3", parentID = "1143" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", sortOrder = "1", parentID = "1143" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "my name 3", sortOrder = "2", parentID = "1143" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "my name 4", bodyText = "lorem ipsum", parentID = "2222" })
                    });

                var searcher = indexer.Searcher;

                var sc = searcher.CreateQuery("content")
                    .Facet("sortOrder")
                    .And()
                    .Facet("parentID")
                    .And();
                var sc1 = sc.Field("parentID", 1143).OrderBy(new SortableField("sortOrder", SortType.Int));

                var results1 = sc1.Execute();

                var facetResults = results1.GetFacets();

                Assert.AreEqual(3, results1.Count());
                Assert.AreEqual(2, facetResults.Count());
                Assert.AreEqual(3, facetResults.First().Count());
                Assert.AreEqual(1, facetResults.Last().Count());
                Assert.AreEqual(3, facetResults.Last().Facet("1143").Value);

                var results2 = results1.ToArray();
                var currSort = 0;
                for (var i = 0; i < results2.Length; i++)
                {
                    Assert.GreaterOrEqual(int.Parse(results2[i].Values["sortOrder"]), currSort);
                    currSort = int.Parse(results2[i].Values["sortOrder"]);
                }
            }
        }

        [Test]
        public void Sort_Result_By_Date_Field_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                //Ensure it's set to a date, otherwise it's not sortable
                new FieldDefinitionCollection(new FieldDefinition("updateDate", FieldDefinitionTypes.FacetDateTime), new FieldDefinition("parentID", FieldDefinitionTypes.FacetInteger))))
            {


                var now = DateTime.Now;

                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", updateDate = now.AddDays(2).ToString("yyyy-MM-dd"), parentID = "1143" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", updateDate = now.ToString("yyyy-MM-dd"), parentID = 1143 }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "my name 3", updateDate = now.AddDays(1).ToString("yyyy-MM-dd"), parentID = 1143 }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "my name 4", updateDate = now, parentID = "2222" })
                    });

                var searcher = indexer.Searcher;

                var sc = searcher.CreateQuery("content")
                    .Facet("updateDate")
                    .And()
                    .Facet("parentID")
                    .And();
                //note: dates internally are stored as Long, see DateTimeType
                var sc1 = sc.Field("parentID", 1143).OrderBy(new SortableField("updateDate", SortType.Long));

                var results1 = sc1.Execute();

                var facetResults = results1.GetFacet("updateDate");
                var facetReuslts2 = results1.GetFacet("parentID");

                Assert.AreEqual(3, results1.Count());
                Assert.AreEqual(3, facetResults.Count());
                Assert.AreEqual(1, facetReuslts2.Count());

                var results2 = results1.ToArray();
                double currSort = 0;
                for (var i = 0; i < results2.Length; i++)
                {
                    Assert.GreaterOrEqual(double.Parse(results2[i].Values["updateDate"]), currSort);
                    currSort = double.Parse(results2[i].Values["updateDate"]);
                }
            }
        }

        [Test]
        public void Sort_Result_By_Single_Field_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                //Ensure it's set to a fulltextsortable, otherwise it's not sortable
                new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullTextSortable))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", writerName = "administrator", parentID = "1143" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", writerName = "administrator", parentID = "1143" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "my name 3", writerName = "administrator", parentID = "1143" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "my name 4", writerName = "writer", parentID = "2222" })
                    });

                var searcher = indexer.Searcher;

                var sc = searcher.CreateQuery("content")
                    .Facet("nodeName").And();
                var sc1 = sc.Field("writerName", "administrator")
                    .OrderBy(new SortableField("nodeName", SortType.String));

                sc = searcher.CreateQuery("content")
                    .Facet("nodeName").And();
                var sc2 = sc.Field("writerName", "administrator")
                    .OrderByDescending(new SortableField("nodeName", SortType.String));

                var results1 = sc1.Execute();
                var results2 = sc2.Execute();

                var facetResults1 = results1.GetFacet("nodeName");
                var facetResults2 = results2.GetFacet("nodeName");

                Assert.AreNotEqual(results1.First().Id, results2.First().Id);

                Assert.AreEqual(3, facetResults1.Count());
                Assert.AreEqual(3, facetResults2.Count());
            }


        }

        [TestCase(FieldDefinitionTypes.FacetDouble, SortType.Double)]
        [TestCase(FieldDefinitionTypes.FacetFullText, SortType.Double)]
        [TestCase(FieldDefinitionTypes.FacetFullText, SortType.String)]
        [TestCase(FieldDefinitionTypes.FacetFullTextSortable, SortType.Double)]
        [TestCase(FieldDefinitionTypes.FacetFullTextSortable, SortType.String)]
        public void Sort_Result_By_Double_Fields_Facet(string fieldType, SortType sortType)
        {
            // See: https://github.com/Shazwazza/Examine/issues/242

            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(
                    new FieldDefinition("field1", fieldType))))
            {
                indexer.IndexItems(new[]
                {
                    ValueSet.FromObject(1.ToString(), "content", new { field1 = 5.0 }),
                    ValueSet.FromObject(2.ToString(), "content", new { field1 = 4.9 }),
                    ValueSet.FromObject(3.ToString(), "content", new { field1 = 4.5 }),
                    ValueSet.FromObject(4.ToString(), "content", new { field1 = 3.9 }),
                    ValueSet.FromObject(5.ToString(), "content", new { field1 = 3.8 }),
                    ValueSet.FromObject(6.ToString(), "content", new { field1 = 2.6 }),
                });

                var searcher = indexer.Searcher;

                var sc = searcher.CreateQuery("content").Facet("field1").And();
                var sc1 = sc.All()
                    .OrderBy(new SortableField("field1", sortType));

                sc = searcher.CreateQuery("content").Facet("field1").And();
                var sc2 = sc.All()
                    .OrderByDescending(new SortableField("field1", sortType));

                var results1 = sc1.Execute();
                var results2 = sc2.Execute();

                var facetResults1 = results1.GetFacet("field1");
                var facetResults2 = results2.GetFacet("field1");

                var results3 = results1.ToList();
                var results4 = results2.ToList();

                Assert.AreEqual(2.6, double.Parse(results3[0].Values["field1"]));
                Assert.AreEqual(3.8, double.Parse(results3[1].Values["field1"]));
                Assert.AreEqual(3.9, double.Parse(results3[2].Values["field1"]));
                Assert.AreEqual(4.5, double.Parse(results3[3].Values["field1"]));
                Assert.AreEqual(4.9, double.Parse(results3[4].Values["field1"]));
                Assert.AreEqual(5.0, double.Parse(results3[5].Values["field1"]));


                Assert.AreEqual(2.6, double.Parse(results4[5].Values["field1"]));
                Assert.AreEqual(3.8, double.Parse(results4[4].Values["field1"]));
                Assert.AreEqual(3.9, double.Parse(results4[3].Values["field1"]));
                Assert.AreEqual(4.5, double.Parse(results4[2].Values["field1"]));
                Assert.AreEqual(4.9, double.Parse(results4[1].Values["field1"]));
                Assert.AreEqual(5.0, double.Parse(results4[0].Values["field1"]));

                Assert.AreEqual(6, facetResults1.Count());
                Assert.AreEqual(6, facetResults2.Count());
            }
        }

        [Test]
        public void Sort_Result_By_Multiple_Fields_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(
                    new FieldDefinition("field1", FieldDefinitionTypes.FacetDouble),
                    new FieldDefinition("field2", FieldDefinitionTypes.FacetInteger))))
            {
                indexer.IndexItems(new[]
                {
                    ValueSet.FromObject(1.ToString(), "content", new { field1 = 5.0, field2 = 2 }),
                    ValueSet.FromObject(2.ToString(), "content", new { field1 = 4.9, field2 = 2 }),
                    ValueSet.FromObject(3.ToString(), "content", new { field1 = 4.5, field2 = 2 }),
                    ValueSet.FromObject(4.ToString(), "content", new { field1 = 3.9, field2 = 1 }),
                    ValueSet.FromObject(5.ToString(), "content", new { field1 = 3.8, field2 = 1 }),
                    ValueSet.FromObject(6.ToString(), "content", new { field1 = 2.6, field2 = 1 }),
                });

                var searcher = indexer.Searcher;

                var sc = searcher.CreateQuery("content").Facet("field1").And().Facet("field2").And();
                var sc1 = sc.All()
                    .OrderByDescending(new SortableField("field2", SortType.Int))
                    .OrderBy(new SortableField("field1", SortType.Double));

                var results1 = sc1.Execute();

                var facetResults = results1.GetFacet("field1");
                var facetResults2 = results1.GetFacet("field2");

                var results2 = results1.ToList();
                Assert.AreEqual("3", results2[0].Id);
                Assert.AreEqual("2", results2[1].Id);
                Assert.AreEqual("1", results2[2].Id);
                Assert.AreEqual("6", results2[3].Id);
                Assert.AreEqual("5", results2[4].Id);
                Assert.AreEqual("4", results2[5].Id);

                Assert.AreEqual(6, facetResults.Count());
                Assert.AreEqual(2, facetResults2.Count());
            }
        }

        [Test]
        public void Standard_Results_Sorted_By_Score_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("bodyText", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "umbraco", headerText = "world", bodyText = "blah" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "umbraco", headerText = "umbraco", bodyText = "blah" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "umbraco", headerText = "umbraco", bodyText = "umbraco" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "hello", headerText = "world", bodyText = "blah" })
                    });

                var searcher = indexer.Searcher;

                var sc = searcher.CreateQuery("content", BooleanOperation.Or).Facet("bodyText").And();
                var sc1 = sc.Field("nodeName", "umbraco").Or().Field("headerText", "umbraco").Or().Field("bodyText", "umbraco");

                var results = sc1.Execute();

                var facetResults = results.GetFacet("bodyText");

                Assert.AreEqual(2, facetResults.Count());

                //Assert
                for (int i = 0; i < results.TotalItemCount - 1; i++)
                {
                    var curr = results.ElementAt(i);
                    var next = results.ElementAtOrDefault(i + 1);

                    if (next == null)
                        break;

                    Assert.IsTrue(curr.Score >= next.Score, string.Format("Result at index {0} must have a higher score than result at index {1}", i, i + 1));
                }
            }

        }

        [Test]
        public void Skip_Results_Returns_Different_Results_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
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

                var searcher = indexer.Searcher;

                //Arrange
                var sc = searcher.CreateQuery("content").Field("writerName", "administrator").And().Facet("nodeName");

                //Act
                var results = sc.Execute();

                var facetResults = results.GetFacet("nodeName");

                //Assert
                Assert.AreNotEqual(results.First(), results.Skip(2).First(), "Third result should be different");
                Assert.AreEqual(1, facetResults.Count());
            }
        }

        [Test]
        public void Escaping_Includes_All_Words_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "codegarden09", headerText = "world", writerName = "administrator" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "codegarden 09", headerText = "umbraco", writerName = "administrator" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "codegarden  09", headerText = "umbraco", writerName = "administrator" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "codegarden 090", headerText = "world", writerName = "blah" })
                    });

                var searcher = indexer.Searcher;

                //Arrange
                var sc = searcher.CreateQuery("content").Field("nodeName", "codegarden 09".Escape()).And().Facet("nodeName");

                Console.WriteLine(sc.ToString());

                //Act
                var results = sc.Execute();

                var facetResults = results.GetFacet("nodeName");

                //Assert
                //NOTE: The result is 2 because the double space is removed with the analyzer
                Assert.AreEqual(2, results.TotalItemCount);
                Assert.AreEqual(2, facetResults.Count());
            }


        }

        [Test]
        public void Grouped_And_Examiness_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "Aloha", nodeTypeAlias = "CWS_Hello" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "Helo", nodeTypeAlias = "CWS_World" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "Another node", nodeTypeAlias = "SomethingElse" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "Always consider this", nodeTypeAlias = "CWS_World" })
                    });

                var searcher = indexer.Searcher;

                //Arrange
                var criteria = searcher.CreateQuery("content").Facet("nodeName").And();

                //get all node type aliases starting with CWS and all nodees starting with "A"
                var filter = criteria.GroupedAnd(
                    new[] { "nodeTypeAlias", "nodeName" },
                    new[] { "CWS".MultipleCharacterWildcard(), "A".MultipleCharacterWildcard() });


                //Act
                var results = filter.Execute();

                var facetResults = results.GetFacet("nodeName");

                //Assert
                Assert.AreEqual(2, results.TotalItemCount);
                Assert.AreEqual(2, facetResults.Count());
            }
        }

        [Test]
        public void Examiness_Proximity_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "Aloha", metaKeywords = "Warren is likely to be creative" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "Helo", metaKeywords = "Creative is Warren middle name" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "Another node", metaKeywords = "If Warren were creative... well, he actually is" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "Always consider this", metaKeywords = "Warren is a very talented individual and quite creative" })
                    });

                var searcher = indexer.Searcher;

                //Arrange
                var criteria = searcher.CreateQuery("content").Facet("nodeName").And();

                //get all nodes that contain the words warren and creative within 5 words of each other
                var filter = criteria.Field("metaKeywords", "Warren creative".Proximity(5));

                //Act
                var results = filter.Execute();

                var facetResults = results.GetFacet("nodeName");

                foreach (var r in results)
                {
                    Console.WriteLine($"Id = {r.Id}");
                }

                //Assert
                Assert.AreEqual(3, results.TotalItemCount);
                Assert.AreEqual(3, facetResults.Count());
            }
        }

        /// <summary>
        /// test range query with a Float structure
        /// </summary>
        [Test]
        public void Float_Range_SimpleIndexSet()
        {

            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                //Ensure it's set to a float
                new FieldDefinitionCollection(new FieldDefinition("SomeFloat", FieldDefinitionTypes.FacetFloat))))
            {


                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "Aloha", SomeFloat = 1 }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "Helo", SomeFloat = 123 }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "Another node", SomeFloat = 12 }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "Always consider this", SomeFloat = 25 })
                    });

                var searcher = indexer.Searcher;

                //all numbers should be between 0 and 100 based on the data source
                var criteria1 = searcher.CreateQuery().Facet("SomeFloat", new DoubleRange[]
                {
                    new DoubleRange("1", 0, true, 12, true),
                    new DoubleRange("2", 13, true, 250, true)
                }).IsFloat(true).And();
                var filter1 = criteria1.RangeQuery<float>(new[] { "SomeFloat" }, 0f, 100f, true, true);

                var criteria2 = searcher.CreateQuery().Facet("SomeFloat", new DoubleRange[]
                {
                    new DoubleRange("1", 0, true, 12, true),
                    new DoubleRange("2", 13, true, 250, true)
                }).IsFloat(true).And();
                var filter2 = criteria2.RangeQuery<float>(new[] { "SomeFloat" }, 101f, 200f, true, true);

                //Act
                var results1 = filter1.Execute();
                var results2 = filter2.Execute();

                var facetResults1 = results1.GetFacet("SomeFloat");
                var facetResults2 = results2.GetFacet("SomeFloat");

                //Assert
                Assert.AreEqual(3, results1.TotalItemCount);
                Assert.AreEqual(1, results2.TotalItemCount);
                Assert.AreEqual(2, facetResults1.Facet("1").Value);
                Assert.AreEqual(1, facetResults1.Facet("2").Value);
                Assert.AreEqual(0, facetResults2.Facet("1").Value);
                Assert.AreEqual(1, facetResults2.Facet("2").Value);
            }


        }

        /// <summary>
        /// test range query with a Number structure
        /// </summary>
        [Test]
        public void Number_Range_SimpleIndexSet_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                //Ensure it's set to a float
                new FieldDefinitionCollection(new FieldDefinition("SomeNumber", FieldDefinitionTypes.FacetInteger))))
            {


                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "Aloha", SomeNumber = 1 }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "Helo", SomeNumber = 123 }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "Another node", SomeNumber = 12 }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "Always consider this", SomeNumber = 25 })
                    });

                var searcher = indexer.Searcher;

                //all numbers should be between 0 and 100 based on the data source
                var criteria1 = searcher.CreateQuery().Facet("SomeNumber").MaxCount(1).And();
                var filter1 = criteria1.RangeQuery<int>(new[] { "SomeNumber" }, 0, 100, true, true);

                var criteria2 = searcher.CreateQuery().Facet("SomeNumber").MaxCount(1).And();
                var filter2 = criteria2.RangeQuery<int>(new[] { "SomeNumber" }, 101, 200, true, true);

                //Act
                var results1 = filter1.Execute();
                var results2 = filter2.Execute();

                var facetResults1 = results1.GetFacet("SomeNumber");
                var facetResults2 = results2.GetFacet("SomeNumber");

                //Assert
                Assert.AreEqual(3, results1.TotalItemCount);
                Assert.AreEqual(1, results2.TotalItemCount);
                Assert.AreEqual(1, facetResults1.Count());
                Assert.AreEqual(1, facetResults2.Count());
            }
        }

        /// <summary>
        /// test range query with a Number structure
        /// </summary>
        [Test]
        public void Double_Range_SimpleIndexSet_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                //Ensure it's set to a float
                new FieldDefinitionCollection(new FieldDefinition("SomeDouble", FieldDefinitionTypes.FacetDouble))))
            {


                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "Aloha", SomeDouble = 1d }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "Helo", SomeDouble = 123d }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "Another node", SomeDouble = 12d }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "Always consider this", SomeDouble = 25d })
                    });

                var searcher = indexer.Searcher;

                //all numbers should be between 0 and 100 based on the data source
                var criteria1 = searcher.CreateQuery().Facet("SomeDouble", new DoubleRange[]
                {
                    new DoubleRange("1", 0, true, 100, true),
                    new DoubleRange("2", 101, true, 200, true)
                }).And();
                var filter1 = criteria1.RangeQuery<double>(new[] { "SomeDouble" }, 0d, 100d, true, true);

                var criteria2 = searcher.CreateQuery("content").Facet("SomeDouble", new DoubleRange[]
                {
                    new DoubleRange("1", 0, true, 100, true),
                    new DoubleRange("2", 101, true, 200, true)
                }).And();
                var filter2 = criteria2.RangeQuery<double>(new[] { "SomeDouble" }, 101d, 200d, true, true);

                //Act
                var results1 = filter1.Execute();
                var results2 = filter2.Execute();

                var facetResults1 = results1.GetFacet("SomeDouble");
                var facetResults2 = results2.GetFacet("SomeDouble");

                //Assert
                Assert.AreEqual(3, results1.TotalItemCount);
                Assert.AreEqual(1, results2.TotalItemCount);
                Assert.AreEqual(3, facetResults1.Facet("1").Value);
                Assert.AreEqual(0, facetResults1.Facet("2").Value);
                Assert.AreEqual(0, facetResults2.Facet("1").Value);
                Assert.AreEqual(1, facetResults2.Facet("2").Value);
            }
        }

        /// <summary>
        /// test range query with a Double structure
        /// </summary>
        [Test]
        public void Long_Range_SimpleIndexSet_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                //Ensure it's set to a float
                new FieldDefinitionCollection(new FieldDefinition("SomeLong", FieldDefinitionTypes.FacetLong))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "Aloha", SomeLong = 1L }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "Helo", SomeLong = 123L }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "Another node", SomeLong = 12L }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "Always consider this", SomeLong = 25L })
                    });

                var searcher = indexer.Searcher;

                //all numbers should be between 0 and 100 based on the data source
                var criteria1 = searcher.CreateQuery().Facet("SomeLong", new Int64Range[]
                {
                    new Int64Range("1", 0L, true, 100L, true),
                    new Int64Range("2", 101L, true, 200L, true)
                }).And();
                var filter1 = criteria1.RangeQuery<long>(new[] { "SomeLong" }, 0L, 100L, true, true);

                var criteria2 = searcher.CreateQuery().Facet("SomeLong", new Int64Range[]
                {
                    new Int64Range("1", 0L, true, 100L, true),
                    new Int64Range("2", 101L, true, 200L, true)
                }).And();
                var filter2 = criteria2.RangeQuery<long>(new[] { "SomeLong" }, 101L, 200L, true, true);

                //Act
                var results1 = filter1.Execute();
                var results2 = filter2.Execute();

                var facetResults1 = results1.GetFacet("SomeLong");
                var facetResults2 = results2.GetFacet("SomeLong");

                //Assert
                Assert.AreEqual(3, results1.TotalItemCount);
                Assert.AreEqual(1, results2.TotalItemCount);
                Assert.AreEqual(3, facetResults1.Facet("1").Value);
                Assert.AreEqual(0, facetResults1.Facet("2").Value);
                Assert.AreEqual(0, facetResults2.Facet("1").Value);
                Assert.AreEqual(1, facetResults2.Facet("2").Value);
            }
        }



        /// <summary>
        /// Test range query with a DateTime structure
        /// </summary>
        [Test]
        public void Date_Range_SimpleIndexSet_Facet()
        {
            var reIndexDateTime = DateTime.Now;

            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("DateCreated", FieldDefinitionTypes.FacetDateTime))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { DateCreated = reIndexDateTime }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { DateCreated = reIndexDateTime }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { DateCreated = reIndexDateTime.AddMonths(-10) }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { DateCreated = reIndexDateTime })
                    });

                var searcher = indexer.Searcher;

                var criteria = searcher.CreateQuery().Facet("DateCreated", new Int64Range[]
                {
                    new Int64Range("1", reIndexDateTime.AddYears(-1).Ticks, true, reIndexDateTime.Ticks, true),
                    new Int64Range("2", reIndexDateTime.AddMinutes(1).Ticks, true, reIndexDateTime.AddDays(1).Ticks, true)
                }).And();
                var filter = criteria.RangeQuery<DateTime>(new[] { "DateCreated" }, reIndexDateTime, DateTime.Now, true, true);

                var criteria2 = searcher.CreateQuery().Facet("DateCreated", new Int64Range[]
                {
                    new Int64Range("1", reIndexDateTime.AddYears(-1).Ticks, true, reIndexDateTime.Ticks, true),
                    new Int64Range("2", reIndexDateTime.AddMinutes(1).Ticks, true, reIndexDateTime.AddDays(1).Ticks, true)
                }).And();
                var filter2 = criteria2.RangeQuery<DateTime>(new[] { "DateCreated" }, reIndexDateTime.AddDays(-1), reIndexDateTime.AddSeconds(-1), true, true);

                ////Act
                var results = filter.Execute();
                var results2 = filter2.Execute();

                var facetResults1 = results.GetFacet("DateCreated");
                var facetResults2 = results2.GetFacet("DateCreated");

                ////Assert
                Assert.IsTrue(results.TotalItemCount > 0);
                Assert.IsTrue(results2.TotalItemCount == 0);
                Assert.AreEqual(3, facetResults1.Facet("1").Value);
                Assert.AreEqual(0, facetResults1.Facet("2").Value);
                Assert.AreEqual(0, facetResults2.Facet("1").Value);
                Assert.AreEqual(0, facetResults2.Facet("2").Value);
            }


        }

        [Test]
        public void Fuzzy_Search_Facet()
        {
            var analyzer = new EnglishAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("Content", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { Content = "I'm thinking here" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "I'm a thinker" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { Content = "I am pretty thoughtful" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { Content = "I thought you were cool" })
                    });

                var searcher = indexer.Searcher;

                var criteria = searcher.CreateQuery().Facet("Content").And();
                var filter = criteria.Field("Content", "think".Fuzzy(0.1F));

                var criteria2 = searcher.CreateQuery().Facet("Content").And();
                var filter2 = criteria2.Field("Content", "thought".Fuzzy());

                Console.WriteLine(filter);
                Console.WriteLine(filter2);

                ////Act
                var results = filter.Execute();
                var results2 = filter2.Execute();

                var facetResults1 = results.GetFacet("Content");
                var facetResults2 = results2.GetFacet("Content");

                foreach (var r in results)
                {
                    Console.WriteLine($"Result Id: {r.Id}");
                }

                foreach (var r in results2)
                {
                    Console.WriteLine($"Result2 Id: {r.Id}");
                }

                ////Assert
                Assert.AreEqual(2, results.TotalItemCount);
                Assert.AreEqual(2, results2.TotalItemCount);
                Assert.AreEqual(2, facetResults1.Count());
                Assert.AreEqual(2, facetResults2.Count());
            }
        }

        [Test]
        public void Inner_Or_Query_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("Type", FieldDefinitionTypes.FacetFullText))))


            {


                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { Content = "hello world", Type = "type1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "hello something or other", Type = "type1" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { Content = "hello you cruel world", Type = "type2" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { Content = "hi there, hello world", Type = "type2" })
                    });

                var searcher = indexer.Searcher;

                var criteria = searcher.CreateQuery().Facet("Type").And();

                //Query = 
                //  +Type:type1 +(Content:world Content:something)

                var filter = criteria.Field("Type", "type1")
                    .And(query => query.Field("Content", "world").Or().Field("Content", "something"), BooleanOperation.Or);

                //Act
                var results = filter.Execute();

                var facetResults = results.GetFacet("Type");

                //Assert
                Assert.AreEqual(2, results.TotalItemCount);
                Assert.AreEqual(1, facetResults.Count());
            }
        }

        [Test]
        public void Inner_And_Query_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("Type", FieldDefinitionTypes.FacetFullText))))


            {


                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { Content = "hello world", Type = "type1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "hello something or other", Type = "type1" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { Content = "hello something or world", Type = "type1" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { Content = "hello you cruel world", Type = "type2" }),
                    ValueSet.FromObject(5.ToString(), "content",
                        new { Content = "hi there, hello world", Type = "type2" })
                    });

                var searcher = indexer.Searcher;

                var criteria = searcher.CreateQuery().Facet("Type").And();

                //Query = 
                //  +Type:type1 +(+Content:world +Content:hello)

                var filter = criteria.Field("Type", "type1")
                    .And(query => query.Field("Content", "world").And().Field("Content", "hello"));

                //Act
                var results = filter.Execute();

                var facetResults = results.GetFacet("Type");

                //Assert
                Assert.AreEqual(2, results.TotalItemCount);
                Assert.AreEqual(1, facetResults.Count());
            }
        }

        [Test]
        public void Inner_Not_Query_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("Type", FieldDefinitionTypes.FacetFullText))))


            {


                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { Content = "hello world", Type = "type1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "hello something or other", Type = "type1" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { Content = "hello something or world", Type = "type1" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { Content = "hello you cruel world", Type = "type2" }),
                    ValueSet.FromObject(5.ToString(), "content",
                        new { Content = "hi there, hello world", Type = "type2" })
                    });

                var searcher = indexer.Searcher;

                var criteria = searcher.CreateQuery().Facet("Type").And();

                //Query = 
                //  +Type:type1 +(+Content:world -Content:something)

                var filter = criteria.Field("Type", "type1")
                    .And(query => query.Field("Content", "world").Not().Field("Content", "something"));

                //Act
                var results = filter.Execute();

                var facetResults = results.GetFacet("Type");

                //Assert
                Assert.AreEqual(1, results.TotalItemCount);
                Assert.AreEqual(1, facetResults.Count());
            }
        }

        [Test]
        public void Complex_Or_Group_Nested_Query_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("Type", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { Content = "hello world", Type = "type1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "hello something or other", Type = "type1" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { Content = "hello you guys", Type = "type1" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { Content = "hello you cruel world", Type = "type2" }),
                    ValueSet.FromObject(5.ToString(), "content",
                        new { Content = "hi there, hello world", Type = "type2" })
                    });

                var searcher = indexer.Searcher;

                var criteria = searcher.CreateQuery(defaultOperation: BooleanOperation.Or);

                //Query = 
                //  (+Type:type1 +(Content:world Content:something)) (+Type:type2 +(+Content:world +Content:cruel))

                var filter = criteria
                    .Group(group => group.Field("Type", "type1")
                        .And(query => query.Field("Content", "world").Or().Field("Content", "something"), BooleanOperation.Or),
                        // required so that the type1 query is required
                        BooleanOperation.And)
                    .Or()
                    .Group(group => group.Field("Type", "type2")
                        .And(query => query.Field("Content", "world").And().Field("Content", "cruel")),
                        // required so that the type2 query is required
                        BooleanOperation.And)
                    .And()
                    .Facet("Type");

                Console.WriteLine(filter);

                //Act
                var results = filter.Execute();

                var facetResults = results.GetFacet("Type");

                //Assert
                foreach (var r in results)
                {
                    Console.WriteLine($"Result Id: {r.Id}");
                }
                Assert.AreEqual(3, results.TotalItemCount);

                Assert.AreEqual(2, facetResults.Count());
            }
        }


        [Test]
        public void Custom_Lucene_Query_With_Native_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer))
            {
                var searcher = indexer.Searcher;
                var criteria = (LuceneSearchQuery)searcher.CreateQuery();

                //combine a custom lucene query with raw lucene query
                var op = criteria.NativeQuery("hello:world").And().Facet("SomeFacet").And();

                criteria.LuceneQuery(NumericRangeQuery.NewInt64Range("numTest", 4, 5, true, true));

                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+hello:world +numTest:[4 TO 5]", criteria.Query.ToString());
            }
        }

        [Test]
        public void Select_Field()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))

            {
                indexer.IndexItems(new[] {
                    new ValueSet(1.ToString(), "content",
                        new Dictionary<string, object>
                        {
                            {"id","1" },
                            {"nodeName", "my name 1"},
                            {"bodyText", "lorem ipsum"},
                            {"__Path", "-1,123,456,789"}
                        }),
                    new ValueSet(2.ToString(), "content",
                        new Dictionary<string, object>
                        {
                            {"id","2" },
                            {"nodeName", "my name 2"},
                            {"bodyText", "lorem ipsum"},
                            {"__Path", "-1,123,456,987"}
                        })
                    });

                var searcher = indexer.Searcher;
                var sc = searcher.CreateQuery("content").Facet("nodeName").And();
                var sc1 = sc.Field("nodeName", "my name 1").SelectField("__Path");

                var results = sc1.Execute();
                var facetResults = results.GetFacet("nodeName");

                var expectedLoadedFields = new string[] { "__Path" };
                var keys = results.First().Values.Keys.ToArray();
                Assert.True(keys.All(x => expectedLoadedFields.Contains(x)));
                Assert.True(expectedLoadedFields.All(x => keys.Contains(x)));

                Assert.AreEqual(2, facetResults.Count());
            }
        }

        [Test]
        public void Select_Fields_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))

            {
                indexer.IndexItems(new[] {
                    new ValueSet(1.ToString(), "content",
                        new Dictionary<string, object>
                        {
                            {"id","1" },
                            {"nodeName", "my name 1"},
                            {"bodyText", "lorem ipsum"},
                            {"__Path", "-1,123,456,789"}
                        }),
                    new ValueSet(2.ToString(), "content",
                        new Dictionary<string, object>
                        {
                            {"id","2" },
                            {"nodeName", "my name 2"},
                            {"bodyText", "lorem ipsum"},
                            {"__Path", "-1,123,456,987"}
                        })
                    });

                var searcher = indexer.Searcher;
                var sc = searcher.CreateQuery("content").Facet("nodeName").And();
                var sc1 = sc.Field("nodeName", "my name 1").SelectFields(new HashSet<string>(new[] { "nodeName", "bodyText", "id", "__NodeId" }));

                var results = sc1.Execute();
                var facetResults = results.GetFacet("nodeName");

                var expectedLoadedFields = new string[] { "nodeName", "bodyText", "id", "__NodeId" };
                var keys = results.First().Values.Keys.ToArray();
                Assert.True(keys.All(x => expectedLoadedFields.Contains(x)));
                Assert.True(expectedLoadedFields.All(x => keys.Contains(x)));

                Assert.AreEqual(2, facetResults.Count());
            }

        }

        [Test]
        public void Select_Fields_HashSet_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))

            {
                indexer.IndexItems(new[] {
                    new ValueSet(1.ToString(), "content",
                        new Dictionary<string, object>
                        {
                            {"id","1" },
                            {"nodeName", "my name 1"},
                            {"bodyText", "lorem ipsum"},
                            {"__Path", "-1,123,456,789"}
                        }),
                    new ValueSet(2.ToString(), "content",
                        new Dictionary<string, object>
                        {
                            {"id","2" },
                            {"nodeName", "my name 2"},
                            {"bodyText", "lorem ipsum"},
                            {"__Path", "-1,123,456,987"}
                        })
                    });

                var searcher = indexer.Searcher;
                var sc = searcher.CreateQuery("content").Facet("nodeName").And();
                var sc1 = sc.Field("nodeName", "my name 1").SelectFields(new HashSet<string>(new string[] { "nodeName", "bodyText" }));

                var results = sc1.Execute();
                var facetResults = results.GetFacet("nodeName");

                var expectedLoadedFields = new string[] { "nodeName", "bodyText" };
                var keys = results.First().Values.Keys.ToArray();
                Assert.True(keys.All(x => expectedLoadedFields.Contains(x)));
                Assert.True(expectedLoadedFields.All(x => keys.Contains(x)));

                Assert.AreEqual(2, facetResults.Count());
            }
        }

        [Test]
        public void Paging_With_Skip_Take_Facet()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("writerName", FieldDefinitionTypes.FacetFullText))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "umbraco", headerText = "world", writerName = "administrator" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "umbraco", headerText = "umbraco", writerName = "administrator" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "umbraco", headerText = "umbraco", writerName = "administrator" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "hello", headerText = "world", writerName = "blah" }),
                    ValueSet.FromObject(5.ToString(), "content",
                        new { nodeName = "umbraco", headerText = "umbraco", writerName = "administrator" }),
                    ValueSet.FromObject(6.ToString(), "content",
                        new { nodeName = "umbraco", headerText = "umbraco", writerName = "administrator" })
                    });

                var searcher = indexer.Searcher;

                //Arrange

                var sc = searcher.CreateQuery("content").Field("writerName", "administrator").And().Facet("writerName");
                int pageIndex = 0;
                int pageSize = 2;

                //Act

                var results = sc
                    .Execute(QueryOptions.SkipTake(pageIndex * pageSize, pageSize));
                Assert.AreEqual(2, results.Count());
                var facetResults = results.GetFacet("writerName");
                Assert.AreEqual(1, facetResults.Count());
                Assert.AreEqual(5, facetResults.Facet("administrator").Value);

                pageIndex++;

                results = sc
                    .Execute(QueryOptions.SkipTake(pageIndex * pageSize, pageSize));
                Assert.AreEqual(2, results.Count());
                facetResults = results.GetFacet("writerName");
                Assert.AreEqual(1, facetResults.Count());
                Assert.AreEqual(5, facetResults.Facet("administrator").Value);

                pageIndex++;

                results = sc
                    .Execute(QueryOptions.SkipTake(pageIndex * pageSize, pageSize));
                Assert.AreEqual(1, results.Count());
                facetResults = results.GetFacet("writerName");
                Assert.AreEqual(1, facetResults.Count());
                Assert.AreEqual(5, facetResults.Facet("administrator").Value);

                pageIndex++;

                results = sc
                    .Execute(QueryOptions.SkipTake(pageIndex * pageSize, pageSize));
                Assert.AreEqual(0, results.Count());
                facetResults = results.GetFacet("writerName");
                Assert.AreEqual(1, facetResults.Count());
                Assert.AreEqual(5, facetResults.Facet("administrator").Value);
            }
        }

        [TestCase(0, 1, 1)]
        [TestCase(0, 2, 2)]
        [TestCase(0, 3, 3)]
        [TestCase(0, 4, 4)]
        [TestCase(0, 5, 5)]
        [TestCase(0, 100, 5)]
        [TestCase(1, 1, 1)]
        [TestCase(1, 2, 2)]
        [TestCase(1, 3, 3)]
        [TestCase(1, 4, 4)]
        [TestCase(1, 5, 4)]
        [TestCase(2, 2, 2)]
        [TestCase(2, 5, 3)]
        public void Given_SkipTake_Returns_ExpectedTotals_Facet(int skip, int take, int expectedResults)
        {
            const int indexSize = 5;
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FacetFullText))))
            {
                var items = Enumerable.Range(0, indexSize).Select(x => ValueSet.FromObject(x.ToString(), "content",
                    new { nodeName = "umbraco", headerText = "world", writerName = "administrator" }));

                indexer.IndexItems(items);

                var searcher = indexer.Searcher;

                //Arrange

                var sc = searcher.CreateQuery("content").Field("writerName", "administrator").And().Facet("nodeName");

                //Act

                var results = sc.Execute(QueryOptions.SkipTake(skip, take));

                var facetResults = results.GetFacet("nodeName");

                Assert.AreEqual(indexSize, results.TotalItemCount);
                Assert.AreEqual(expectedResults, results.Count());
                Assert.AreEqual(1, facetResults.Count());
                Assert.AreEqual(5, facetResults.Facet("umbraco").Value);
            }
        }
    }
}
