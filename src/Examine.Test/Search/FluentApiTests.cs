using System;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.Search;
using Examine.Search;
using Examine.Test.UmbracoExamine;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using NUnit.Framework;



namespace Examine.Test.Search
{
    [TestFixture]
    public class FluentApiTests
    {
        [Test]
        public void NativeQuery_Single_Word()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer)),
                luceneDir, analyzer))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia"})
                    });

                var searcher = indexer.GetSearcher();

                var query = searcher.CreateQuery("content").NativeQuery("sydney");

                Console.WriteLine(query);

                var results = query.Execute();

                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void NativeQuery_Phrase()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer)),
                luceneDir, analyzer))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "location 3", bodyText = "In Australia there is a town called Bateau Bay in NSW"})
                    });

                var searcher = indexer.GetSearcher();

                var query = searcher.CreateQuery("content").NativeQuery("\"town called\"");

                Console.WriteLine(query);
                Assert.AreEqual("{ Category: content, LuceneQuery: +(bodyText:\"town called\" nodeName:\"town called\") }", query.ToString());

                var results = query.Execute();

                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void Managed_Range_Date()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                new FieldDefinitionCollection(new FieldDefinition("created", "datetime")),
                luceneDir, analyzer))
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


                var searcher = indexer.GetSearcher();

                var numberSortedCriteria = searcher.CreateQuery()
                    .RangeQuery<DateTime>(new[] { "created" }, new DateTime(2000, 01, 02), new DateTime(2000, 01, 05), maxInclusive: false);

                var numberSortedResult = numberSortedCriteria.Execute();

                Assert.AreEqual(2, numberSortedResult.TotalItemCount);
            }
        }

        [Test]
        public void Managed_Full_Text()
        {
            var analyzer = new StandardAnalyzer(Util.Version);

            using (var luceneDir1 = new RandomIdRAMDirectory())
            using (var indexer1 = new TestIndex(luceneDir1, analyzer))
            {
                indexer1.IndexItem(ValueSet.FromObject("1", "content", new { item1 = "value1", item2 = "The agitated zebras gallop back and forth in short, panicky dashes, then skitter off into the total absolute darkness." }));
                indexer1.IndexItem(ValueSet.FromObject("2", "content", new { item1 = "value2", item2 = "The festival lasts five days and celebrates the victory of good over evil, light over darkness, and knowledge over ignorance." }));
                indexer1.IndexItem(ValueSet.FromObject("3", "content", new { item1 = "value3", item2 = "They are expected to confront the darkness and show evidence that they have done so in their papers" }));
                indexer1.IndexItem(ValueSet.FromObject("4", "content", new { item1 = "value4", item2 = "Scientists believe the lake could be home to cold-loving microbial life adapted to living in total darkness." }));
                indexer1.IndexItem(ValueSet.FromObject("5", "content", new { item1 = "value3", item2 = "Scotch scotch scotch, i love scotch" }));
                indexer1.IndexItem(ValueSet.FromObject("6", "content", new { item1 = "value4", item2 = "60% of the time, it works everytime" }));
                indexer1.IndexItem(ValueSet.FromObject("7", "content", new { SomeField = "value5", AnotherField = "another value" }));

                var searcher = indexer1.GetSearcher();

                var result = searcher.Search("darkness");

                Assert.AreEqual(4, result.TotalItemCount);
                Console.WriteLine("Search 1:");
                foreach (var r in result)
                {
                    Console.WriteLine($"Id = {r.Id}, Score = {r.Score}");
                }

                result = searcher.Search("total darkness");
                Assert.AreEqual(2, result.TotalItemCount);
                Console.WriteLine("Search 2:");
                foreach (var r in result)
                {
                    Console.WriteLine($"Id = {r.Id}, Score = {r.Score}");
                }
            }
        }

        [Test]
        public void Managed_Full_Text_With_Bool()
        {
            var analyzer = new StandardAnalyzer(Util.Version);

            using (var luceneDir1 = new RandomIdRAMDirectory())
            using (var indexer1 = new TestIndex(luceneDir1, analyzer))
            {
                indexer1.IndexItem(ValueSet.FromObject("1", "content", new { item1 = "value1", item2 = "The agitated zebras gallop back and forth in short, panicky dashes, then skitter off into the total absolute darkness." }));
                indexer1.IndexItem(ValueSet.FromObject("2", "content", new { item1 = "value2", item2 = "The festival lasts five days and celebrates the victory of good over evil, light over darkness, and knowledge over ignorance." }));
                indexer1.IndexItem(ValueSet.FromObject("3", "content", new { item1 = "value3", item2 = "They are expected to confront the darkness and show evidence that they have done so in their papers" }));
                indexer1.IndexItem(ValueSet.FromObject("4", "content", new { item1 = "value4", item2 = "Scientists believe the lake could be home to cold-loving microbial life adapted to living in total darkness." }));
                indexer1.IndexItem(ValueSet.FromObject("5", "content", new { item1 = "value3", item2 = "Scotch scotch scotch, i love scotch" }));
                indexer1.IndexItem(ValueSet.FromObject("6", "content", new { item1 = "value4", item2 = "60% of the time, it works everytime" }));

                var searcher = indexer1.GetSearcher();

                var qry = searcher.CreateQuery().ManagedQuery("darkness").And().Field("item1", "value1");
                Console.WriteLine(qry);
                var result = qry.Execute();

                Assert.AreEqual(1, result.TotalItemCount);
                Console.WriteLine("Search 1:");
                foreach (var r in result)
                {
                    Console.WriteLine($"Id = {r.Id}, Score = {r.Score}");
                }

                qry = searcher.CreateQuery().ManagedQuery("darkness")
                    .And(query => query.Field("item1", "value1").Or().Field("item1", "value2"), BooleanOperation.Or);
                Console.WriteLine(qry);
                result = qry.Execute();

                Assert.AreEqual(2, result.TotalItemCount);
                Console.WriteLine("Search 2:");
                foreach (var r in result)
                {
                    Console.WriteLine($"Id = {r.Id}, Score = {r.Score}");
                }
            }
        }

        [Test]
        public void Managed_Range_Int()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer)),
                luceneDir, analyzer))
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

                var searcher = indexer.GetSearcher();

                var numberSortedCriteria = searcher.CreateQuery()
                    .RangeQuery<int>(new[] { "parentID" }, 122, 124);

                var numberSortedResult = numberSortedCriteria.Execute();

                Assert.AreEqual(2, numberSortedResult.TotalItemCount);
            }
        }

        [Test]
        public void Legacy_ParentId()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer)),
                luceneDir, analyzer))
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

                var searcher = indexer.GetSearcher();

                var numberSortedCriteria = searcher.CreateQuery()
                    .Field("parentID", 123)
                    .OrderBy(new SortableField("sortOrder", SortType.Int));

                var numberSortedResult = numberSortedCriteria.Execute();

                Assert.AreEqual(2, numberSortedResult.TotalItemCount);
            }


        }

        [Test]
        public void Grouped_Or_Examiness()
        {
            var analyzer = new SimpleAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))
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

                var searcher = indexer.GetSearcher();

                //paths contain punctuation, we'll escape it and ensure an exact match
                var criteria = searcher.CreateQuery("content");

                //get all node type aliases starting with CWS_Home OR and all nodees starting with "About"
                var filter = criteria.GroupedOr(
                    new[] { "nodeTypeAlias", "nodeName" },
                    new[] { "CWS\\_Home".Boost(10), "About".MultipleCharacterWildcard() });

                Console.WriteLine(filter);

                var results = filter.Execute();
                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void Grouped_Or_Query_Output()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))

            {
                var searcher = indexer.GetSearcher();

                Console.WriteLine("GROUPED OR - SINGLE FIELD, MULTI VAL");
                var criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedOr(new[] { "id" }.ToList(), new[] { "1", "2", "3" });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(id:1 id:2 id:3)", criteria.Query.ToString());

                Console.WriteLine("GROUPED OR - MULTI FIELD, MULTI VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedOr(new[] { "id", "parentID" }.ToList(), new[] { "1", "2", "3" });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(id:1 id:2 id:3 parentID:1 parentID:2 parentID:3)", criteria.Query.ToString());

                Console.WriteLine("GROUPED OR - MULTI FIELD, EQUAL MULTI VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedOr(new[] { "id", "parentID", "blahID" }.ToList(), new[] { "1", "2", "3" });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(id:1 id:2 id:3 parentID:1 parentID:2 parentID:3 blahID:1 blahID:2 blahID:3)", criteria.Query.ToString());

                Console.WriteLine("GROUPED OR - MULTI FIELD, SINGLE VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedOr(new[] { "id", "parentID" }.ToList(), new[] { "1" });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(id:1 parentID:1)", criteria.Query.ToString());

                Console.WriteLine("GROUPED OR - SINGLE FIELD, SINGLE VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedOr(new[] { "id" }.ToList(), new[] { "1" });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(id:1)", criteria.Query.ToString());

            }


        }

        [Test]
        public void Grouped_And_Query_Output()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))

            {
                var searcher = indexer.GetSearcher();
                //new LuceneSearcher("testSearcher", luceneDir, analyzer);

                Console.WriteLine("GROUPED AND - SINGLE FIELD, MULTI VAL");
                var criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedAnd(new[] { "id" }.ToList(), new[] { "1", "2", "3" });
                Console.WriteLine(criteria.Query);
                //We used to assert this, but it must be allowed to do an add on the same field multiple times
                //Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1)", criteria.Query.ToString());
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1 +id:2 +id:3)", criteria.Query.ToString());

                Console.WriteLine("GROUPED AND - MULTI FIELD, EQUAL MULTI VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedAnd(new[] { "id", "parentID", "blahID" }.ToList(), new[] { "1", "2", "3" });
                Console.WriteLine(criteria.Query);
                //The field/value array lengths are equal so we will match the key/value pairs
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1 +parentID:2 +blahID:3)", criteria.Query.ToString());

                Console.WriteLine("GROUPED AND - MULTI FIELD, MULTI VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedAnd(new[] { "id", "parentID" }.ToList(), new[] { "1", "2", "3" });
                Console.WriteLine(criteria.Query);
                //There are more than one field and there are more values than fields, in this case we align the key/value pairs
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1 +parentID:2)", criteria.Query.ToString());

                Console.WriteLine("GROUPED AND - MULTI FIELD, SINGLE VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedAnd(new[] { "id", "parentID" }.ToList(), new[] { "1" });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1 +parentID:1)", criteria.Query.ToString());

                Console.WriteLine("GROUPED AND - SINGLE FIELD, SINGLE VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedAnd(new[] { "id" }.ToList(), new[] { "1" });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1)", criteria.Query.ToString());
            }
        }

        /// <summary>
        /// CANNOT BE A MUST WITH NOT i.e. +(-id:1 -id:2 -id:3)  --> That will not work with the "+"
        /// </summary>
        [Test]
        public void Grouped_Not_Query_Output()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))

            {
                var searcher = indexer.GetSearcher();

                Console.WriteLine("GROUPED NOT - SINGLE FIELD, MULTI VAL");
                var criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedNot(new[] { "id" }.ToList(), new[] { "1", "2", "3" });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias -id:1 -id:2 -id:3", criteria.Query.ToString());

                Console.WriteLine("GROUPED NOT - MULTI FIELD, MULTI VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedNot(new[] { "id", "parentID" }.ToList(), new[] { "1", "2", "3" });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias -id:1 -id:2 -id:3 -parentID:1 -parentID:2 -parentID:3", criteria.Query.ToString());

                Console.WriteLine("GROUPED NOT - MULTI FIELD, EQUAL MULTI VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedNot(new[] { "id", "parentID", "blahID" }.ToList(), new[] { "1", "2", "3" });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias -id:1 -id:2 -id:3 -parentID:1 -parentID:2 -parentID:3 -blahID:1 -blahID:2 -blahID:3", criteria.Query.ToString());

                Console.WriteLine("GROUPED NOT - MULTI FIELD, SINGLE VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedNot(new[] { "id", "parentID" }.ToList(), new[] { "1" });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias -id:1 -parentID:1", criteria.Query.ToString());

                Console.WriteLine("GROUPED NOT - SINGLE FIELD, SINGLE VAL");
                criteria = (LuceneSearchQuery)searcher.CreateQuery();
                criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                criteria.GroupedNot(new[] { "id" }.ToList(), new[] { "1" });
                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias -id:1", criteria.Query.ToString());
            }
        }

        [Test]
        public void Grouped_Not_Single_Field_Single_Value()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                luceneDir, analyzer))
            {

                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", bodyText = "lorem ficus", headerText = "header 1", umbracoNaviHide = "1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "lorem ficus", headerText = "header 2", umbracoNaviHide = "0" })
                    });

                var searcher = indexer.GetSearcher();

                var query = (LuceneSearchQuery)searcher.CreateQuery("content");
                query.GroupedNot(new[] { "umbracoNaviHide" }, 1.ToString());
                Console.WriteLine(query.Query);
                var results = query.Execute();
                Assert.AreEqual(1, results.TotalItemCount);
            }
        }

        [Test]
        public void Grouped_Not_Multi_Field_Single_Value()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                luceneDir, analyzer))
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

                var searcher = indexer.GetSearcher();

                var query = searcher.CreateQuery("content").GroupedNot(new[] { "umbracoNaviHide", "show" }, 1.ToString());
                Console.WriteLine(query);
                var results = query.Execute();
                Assert.AreEqual(1, results.TotalItemCount);
            }
        }

        [Test]
        public void Grouped_Or_With_Not()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(

                //TODO: Making this a number makes the query fail - i wonder how to make it work correctly?
                // It's because the searching is NOT using a managed search
                //new[] { new FieldDefinition("umbracoNaviHide", FieldDefinitionTypes.Integer) }, 

                luceneDir, analyzer))
            {


                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", bodyText = "lorem ipsum", headerText = "header 1", umbracoNaviHide = "1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", headerText = "header 2", umbracoNaviHide = "0" })
                    });

                var searcher = indexer.GetSearcher();

                //paths contain punctuation, we'll escape it and ensure an exact match
                var criteria = searcher.CreateQuery("content");
                var filter = criteria.GroupedOr(new[] { "nodeName", "bodyText", "headerText" }, "ipsum").Not().Field("umbracoNaviHide", "1");
                var results = filter.Execute();
                Assert.AreEqual(1, results.TotalItemCount);
            }
        }

        [Test]
        public void Match_By_Path()
        {
            var analyzer = new StandardAnalyzer(Util.Version);

            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                new FieldDefinitionCollection(new FieldDefinition("__Path", "raw")),
                luceneDir, analyzer))
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



                var searcher = indexer.GetSearcher();

                //paths contain punctuation, we'll escape it and ensure an exact match
                var criteria = searcher.CreateQuery("content");
                var filter = criteria.Field("__Path", "-1,123,456,789");
                var results1 = filter.Execute();
                Assert.AreEqual(0, results1.TotalItemCount);

                //now escape it
                var exactcriteria = searcher.CreateQuery("content");
                var exactfilter = exactcriteria.Field("__Path", "-1,123,456,789".Escape());
                var results2 = exactfilter.Execute();
                Assert.AreEqual(1, results2.TotalItemCount);

                //now try wildcards
                var wildcardcriteria = searcher.CreateQuery("content");
                var wildcardfilter = wildcardcriteria.Field("__Path", "-1,123,456,".MultipleCharacterWildcard());
                var results3 = wildcardfilter.Execute();
                Assert.AreEqual(2, results3.TotalItemCount);
                //not found
                wildcardcriteria = searcher.CreateQuery("content");
                wildcardfilter = wildcardcriteria.Field("__Path", "-1,123,457,".MultipleCharacterWildcard());
                results3 = wildcardfilter.Execute();
                Assert.AreEqual(0, results3.TotalItemCount);
            }


        }

        [Test]
        public void Find_By_ParentId()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer)),
                luceneDir, analyzer))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", bodyText = "lorem ipsum", parentID = "1235" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", parentID = "1139" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "my name 3", bodyText = "lorem ipsum", parentID = "1139" })
                    });

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery("content");
                var filter = criteria.Field("parentID", 1139);

                var results = filter.Execute();

                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void Find_By_ParentId_Native_Query()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer)),
                luceneDir, analyzer))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", bodyText = "lorem ipsum", parentID = "1235" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", parentID = "1139" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "my name 3", bodyText = "lorem ipsum", parentID = "1139" })
                    });

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery("content");

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

                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void Find_By_NodeTypeAlias()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                new FieldDefinitionCollection(new FieldDefinition("nodeTypeAlias", "raw")),
                luceneDir, analyzer))
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



                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery("content");
                var filter = criteria.Field("nodeTypeAlias", "CWS_Home".Escape());

                var results = filter.Execute();


                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void Search_With_Stop_Words()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))


            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "into 1", bodyText = "It was one thing to bring Carmen into it, but Jonathan was another story." }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", bodyText = "Hands shoved backwards into his back pockets, he took slow deliberate steps, as if he had something on his mind." }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "my name 3", bodyText = "Slowly carrying the full cups into the living room, she handed one to Alex." })
                    });

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery();

                // TODO: This isn't testing correctly because the search parser is actually removing stop words to generate the search so we actually
                // end up with an empty search and then by fluke this test passes.

                var filter = criteria.Field("bodyText", "into")
                    .Or().Field("nodeName", "into");

                Console.WriteLine(filter);

                var results = filter.Execute();

                Assert.AreEqual(0, results.TotalItemCount);
            }
        }

        [Test]
        public void Search_Native_Query()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))


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

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery("content");

                var results = criteria.NativeQuery("nodeTypeAlias:CWS_Home").Execute();

                Assert.AreEqual(2, results.TotalItemCount);
            }

        }


        [Test]
        public void Find_Only_Image_Media()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))


            {


                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "media",
                        new { nodeName = "my name 1", bodyText = "lorem ipsum", nodeTypeAlias = "image" }),
                    ValueSet.FromObject(2.ToString(), "media",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", nodeTypeAlias = "image" }),
                    ValueSet.FromObject(3.ToString(), "media",
                        new { nodeName = "my name 3", bodyText = "lorem ipsum", nodeTypeAlias = "file" })
                    });

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery("media");
                var filter = criteria.Field("nodeTypeAlias", "image");

                var results = filter.Execute();

                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void Find_Both_Media_And_Content()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))


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

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery(defaultOperation: BooleanOperation.Or);
                var filter = criteria
                    .Field(ExamineFieldNames.CategoryFieldName, "media")
                    .Or()
                    .Field(ExamineFieldNames.CategoryFieldName, "content");

                var results = filter.Execute();

                Assert.AreEqual(3, results.TotalItemCount);
            }
        }

        [Test]
        public void Sort_Result_By_Number_Field()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                //Ensure it's set to a number, otherwise it's not sortable
                new FieldDefinitionCollection(new FieldDefinition("sortOrder", FieldDefinitionTypes.Integer), new FieldDefinition("parentID", FieldDefinitionTypes.Integer)),
                luceneDir, analyzer))


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

                var searcher = indexer.GetSearcher();

                var sc = searcher.CreateQuery("content");
                var sc1 = sc.Field("parentID", 1143).OrderBy(new SortableField("sortOrder", SortType.Int));

                var results1 = sc1.Execute().ToArray();

                Assert.AreEqual(3, results1.Length);

                var currSort = 0;
                for (var i = 0; i < results1.Length; i++)
                {
                    Assert.GreaterOrEqual(int.Parse(results1[i].Values["sortOrder"]), currSort);
                    currSort = int.Parse(results1[i].Values["sortOrder"]);
                }
            }
        }

        [Test]
        public void Sort_Result_By_Date_Field()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                //Ensure it's set to a date, otherwise it's not sortable
                new FieldDefinitionCollection(new FieldDefinition("updateDate", FieldDefinitionTypes.DateTime), new FieldDefinition("parentID", FieldDefinitionTypes.Integer)),
                luceneDir, analyzer))
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

                var searcher = indexer.GetSearcher();

                var sc = searcher.CreateQuery("content");
                //note: dates internally are stored as Long, see DateTimeType
                var sc1 = sc.Field("parentID", 1143).OrderBy(new SortableField("updateDate", SortType.Long));

                var results1 = sc1.Execute().ToArray();

                Assert.AreEqual(3, results1.Length);

                double currSort = 0;
                for (var i = 0; i < results1.Length; i++)
                {
                    Assert.GreaterOrEqual(double.Parse(results1[i].Values["updateDate"]), currSort);
                    currSort = double.Parse(results1[i].Values["updateDate"]);
                }
            }
        }

        [Test]
        public void Sort_Result_By_Single_Field()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                //Ensure it's set to a fulltextsortable, otherwise it's not sortable
                new FieldDefinitionCollection(new FieldDefinition("nodeName", FieldDefinitionTypes.FullTextSortable)),
                luceneDir, analyzer))


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


                var searcher = indexer.GetSearcher();

                var sc = searcher.CreateQuery("content");
                var sc1 = sc.Field("writerName", "administrator")
                    .OrderBy(new SortableField("nodeName", SortType.String));

                sc = searcher.CreateQuery("content");
                var sc2 = sc.Field("writerName", "administrator")
                    .OrderByDescending(new SortableField("nodeName", SortType.String));

                var results1 = sc1.Execute();
                var results2 = sc2.Execute();

                Assert.AreNotEqual(results1.First().Id, results2.First().Id);
            }


        }

        [Test]
        public void Standard_Results_Sorted_By_Score()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))


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

                var searcher = indexer.GetSearcher();

                var sc = searcher.CreateQuery("content", BooleanOperation.Or);
                var sc1 = sc.Field("nodeName", "umbraco").Or().Field("headerText", "umbraco").Or().Field("bodyText", "umbraco");

                var results = sc1.Execute();

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
        public void Skip_Results_Returns_Different_Results()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
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

                var searcher = indexer.GetSearcher();

                //Arrange
                var sc = searcher.CreateQuery("content").Field("writerName", "administrator");

                //Act
                var results = sc.Execute();

                //Assert
                Assert.AreNotEqual(results.First(), results.Skip(2).First(), "Third result should be different");
            }
        }

        [Test]
        public void Escaping_Includes_All_Words()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))


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

                var searcher = indexer.GetSearcher();

                //Arrange
                var sc = searcher.CreateQuery("content").Field("nodeName", "codegarden 09".Escape());

                //Act
                var results = sc.Execute();

                //Assert
                //NOTE: The result is 2 because the double space is removed with the analyzer
                Assert.AreEqual(2, results.TotalItemCount);
            }


        }

        [Test]
        public void Grouped_And_Examiness()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))


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

                var searcher = indexer.GetSearcher();

                //Arrange
                var criteria = searcher.CreateQuery("content");

                //get all node type aliases starting with CWS and all nodees starting with "A"
                var filter = criteria.GroupedAnd(
                    new[] { "nodeTypeAlias", "nodeName" },
                    new[] { "CWS".MultipleCharacterWildcard(), "A".MultipleCharacterWildcard() });


                //Act
                var results = filter.Execute();

                //Assert
                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void Examiness_Proximity()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))


            {


                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "Aloha", metaKeywords = "Warren is likely to be creative" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "Helo", metaKeywords = "Creative is Warren's middle name" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "Another node", metaKeywords = "If Warren were creative... well, he actually is" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "Always consider this", metaKeywords = "Warren is a very talented individual and quite creative" })
                    });

                var searcher = indexer.GetSearcher();

                //Arrange
                var criteria = searcher.CreateQuery("content");

                //get all nodes that contain the words warren and creative within 5 words of each other
                var filter = criteria.Field("metaKeywords", "Warren creative".Proximity(5));

                //Act
                var results = filter.Execute();

                //Assert
                Assert.AreEqual(3, results.TotalItemCount);
            }


        }

        /// <summary>
        /// test range query with a Float structure
        /// </summary>
        [Test]
        public void Float_Range_SimpleIndexSet()
        {

            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                //Ensure it's set to a float
                new FieldDefinitionCollection(new FieldDefinition("SomeFloat", FieldDefinitionTypes.Float)),
                luceneDir, analyzer))


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

                var searcher = indexer.GetSearcher();

                //all numbers should be between 0 and 100 based on the data source
                var criteria1 = searcher.CreateQuery();
                var filter1 = criteria1.RangeQuery<float>(new[] { "SomeFloat" }, 0f, 100f, true, true);

                var criteria2 = searcher.CreateQuery();
                var filter2 = criteria2.RangeQuery<float>(new[] { "SomeFloat" }, 101f, 200f, true, true);

                //Act
                var results1 = filter1.Execute();
                var results2 = filter2.Execute();

                //Assert
                Assert.AreEqual(3, results1.TotalItemCount);
                Assert.AreEqual(1, results2.TotalItemCount);
            }


        }

        /// <summary>
        /// test range query with a Number structure
        /// </summary>
        [Test]
        public void Number_Range_SimpleIndexSet()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                //Ensure it's set to a float
                new FieldDefinitionCollection(new FieldDefinition("SomeNumber", FieldDefinitionTypes.Integer)),
                luceneDir, analyzer))


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

                var searcher = indexer.GetSearcher();

                //all numbers should be between 0 and 100 based on the data source
                var criteria1 = searcher.CreateQuery();
                var filter1 = criteria1.RangeQuery<int>(new[] { "SomeNumber" }, 0, 100, true, true);

                var criteria2 = searcher.CreateQuery();
                var filter2 = criteria2.RangeQuery<int>(new[] { "SomeNumber" }, 101, 200, true, true);

                //Act
                var results1 = filter1.Execute();
                var results2 = filter2.Execute();

                //Assert
                Assert.AreEqual(3, results1.TotalItemCount);
                Assert.AreEqual(1, results2.TotalItemCount);
            }
        }

        /// <summary>
        /// test range query with a Number structure
        /// </summary>
        [Test]
        public void Double_Range_SimpleIndexSet()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                //Ensure it's set to a float
                new FieldDefinitionCollection(new FieldDefinition("SomeDouble", FieldDefinitionTypes.Double)),
                luceneDir, analyzer))


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

                var searcher = indexer.GetSearcher();

                //all numbers should be between 0 and 100 based on the data source
                var criteria1 = searcher.CreateQuery();
                var filter1 = criteria1.RangeQuery<double>(new[] { "SomeDouble" }, 0d, 100d, true, true);

                var criteria2 = searcher.CreateQuery();
                var filter2 = criteria2.RangeQuery<double>(new[] { "SomeDouble" }, 101d, 200d, true, true);

                //Act
                var results1 = filter1.Execute();
                var results2 = filter2.Execute();

                //Assert
                Assert.AreEqual(3, results1.TotalItemCount);
                Assert.AreEqual(1, results2.TotalItemCount);
            }
        }

        /// <summary>
        /// test range query with a Double structure
        /// </summary>
        [Test]
        public void Long_Range_SimpleIndexSet()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                //Ensure it's set to a float
                new FieldDefinitionCollection(new FieldDefinition("SomeLong", "long")),
                luceneDir, analyzer))


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

                var searcher = indexer.GetSearcher();

                //all numbers should be between 0 and 100 based on the data source
                var criteria1 = searcher.CreateQuery();
                var filter1 = criteria1.RangeQuery<long>(new[] { "SomeLong" }, 0L, 100L, true, true);

                var criteria2 = searcher.CreateQuery();
                var filter2 = criteria2.RangeQuery<long>(new[] { "SomeLong" }, 101L, 200L, true, true);

                //Act
                var results1 = filter1.Execute();
                var results2 = filter2.Execute();

                //Assert
                Assert.AreEqual(3, results1.TotalItemCount);
                Assert.AreEqual(1, results2.TotalItemCount);
            }
        }



        /// <summary>
        /// Test range query with a DateTime structure
        /// </summary>
        [Test]
        public void Date_Range_SimpleIndexSet()
        {
            var reIndexDateTime = DateTime.Now;

            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(

                new FieldDefinitionCollection(new FieldDefinition("DateCreated", "datetime")),
                luceneDir, analyzer))
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

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery();
                var filter = criteria.RangeQuery<DateTime>(new[] { "DateCreated" }, reIndexDateTime, DateTime.Now, true, true);

                var criteria2 = searcher.CreateQuery();
                var filter2 = criteria2.RangeQuery<DateTime>(new[] { "DateCreated" }, reIndexDateTime.AddDays(-1), reIndexDateTime.AddSeconds(-1), true, true);

                ////Act
                var results = filter.Execute();
                var results2 = filter2.Execute();

                ////Assert
                Assert.IsTrue(results.TotalItemCount > 0);
                Assert.IsTrue(results2.TotalItemCount == 0);
            }


        }

        [Test]
        public void Fuzzy_Search()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))


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

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery();
                var filter = criteria.Field("Content", "think".Fuzzy(0.1F));

                var criteria2 = searcher.CreateQuery();
                var filter2 = criteria2.Field("Content", "thought".Fuzzy());

                ////Act
                var results = filter.Execute();
                var results2 = filter2.Execute();

                ////Assert
                Assert.AreEqual(2, results.TotalItemCount);
                Assert.AreEqual(2, results2.TotalItemCount);

            }


        }


        [Test]
        public void Max_Count()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))


            {


                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { Content = "hello world" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "hello worlds" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { Content = "hello you cruel world" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { Content = "hi there, hello world" })
                });

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery();
                var filter = criteria.Field("Content", "hello");

                //Act
                var results = filter.Execute(3);

                //Assert

                Assert.AreEqual(3, results.Count());

                //NOTE: These are the total matched! The actual results are limited
                Assert.AreEqual(4, results.TotalItemCount);

            }

        }


        [Test]
        public void Inner_Or_Query()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))


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

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery();

                //Query = 
                //  +Type:type1 +(Content:world Content:something)

                var filter = criteria.Field("Type", "type1")
                    .And(query => query.Field("Content", "world").Or().Field("Content", "something"), BooleanOperation.Or);

                //Act
                var results = filter.Execute();

                //Assert
                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void Inner_And_Query()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))


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

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery();

                //Query = 
                //  +Type:type1 +(+Content:world +Content:hello)

                var filter = criteria.Field("Type", "type1")
                    .And(query => query.Field("Content", "world").And().Field("Content", "hello"));

                //Act
                var results = filter.Execute();

                //Assert
                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void Inner_Not_Query()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))


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

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery();

                //Query = 
                //  +Type:type1 +(+Content:world -Content:something)

                var filter = criteria.Field("Type", "type1")
                    .And(query => query.Field("Content", "world").Not().Field("Content", "something"));

                //Act
                var results = filter.Execute();

                //Assert
                Assert.AreEqual(1, results.TotalItemCount);
            }
        }

        [Test]
        public void Complex_Or_Group_Nested_Query()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))
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

                var searcher = indexer.GetSearcher();

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
                        BooleanOperation.And);

                Console.WriteLine(filter);

                //Act
                var results = filter.Execute();

                //Assert
                foreach (var r in results)
                {
                    Console.WriteLine($"Result Id: {r.Id}");
                }
                Assert.AreEqual(3, results.TotalItemCount);
            }
        }


        [Test]
        public void Custom_Lucene_Query_With_Native()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))
            {
                var searcher = indexer.GetSearcher();
                var criteria = (LuceneSearchQuery)searcher.CreateQuery();

                //combine a custom lucene query with raw lucene query
                var op = criteria.NativeQuery("hello:world").And();

                criteria.LuceneQuery(NumericRangeQuery.NewInt64Range("numTest", 4, 5, true, true));

                Console.WriteLine(criteria.Query);
                Assert.AreEqual("+hello:world +numTest:[4 TO 5]", criteria.Query.ToString());
            }
        }

        [Test]
        public void Category()
        {
            var analyzer = new StandardAnalyzer(Util.Version);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { Content = "hello world", Type = "type1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "hello something or other", Type = "type1" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { Content = "hello you guys", Type = "type1" }),
                    ValueSet.FromObject(4.ToString(), "media",
                        new { Content = "hello you cruel world", Type = "type2" }),
                    ValueSet.FromObject(5.ToString(), "media",
                        new { Content = "hi there, hello world", Type = "type2" })
                    });

                var searcher = indexer.GetSearcher();

                var query = searcher.CreateQuery("content").ManagedQuery("hello");
                Console.WriteLine(query);

                var results = query.Execute();

                //Assert
                Assert.AreEqual(3, results.TotalItemCount);
            }
        }

        //[Test]
        //public void Wildcard_Results_Sorted_By_Score()
        //{
        //    //Arrange
        //    var sc = _searcher.CreateCriteria("content", SearchCriteria.BooleanOperation.Or);

        //    //set the rewrite method before adding queries
        //    var lsc = (LuceneSearchCriteria)sc;
        //    lsc.QueryParser.MultiTermRewriteMethod = MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE;

        //    sc = sc.NodeName("umbrac".MultipleCharacterWildcard())
        //        .Or().Field("headerText", "umbrac".MultipleCharacterWildcard())
        //        .Or().Field("bodyText", "umbrac".MultipleCharacterWildcard()).Compile();

        //    //Act
        //    var results = _searcher.Search(sc);

        //    Assert.Greater(results.TotalItemCount, 0);

        //    //Assert
        //    for (int i = 0; i < results.TotalItemCount - 1; i++)
        //    {
        //        var curr = results.ElementAt(i);
        //        var next = results.ElementAtOrDefault(i + 1);

        //        if (next == null)
        //            break;

        //        Assert.IsTrue(curr.Score > next.Score, $"Result at index {i} must have a higher score than result at index {i + 1}");
        //    }
        //}

        //[Test]
        //public void Wildcard_Results_Sorted_By_Score_TooManyClauses_Exception()
        //{
        //    //this will throw during rewriting because 'lo*' matches too many things but with the work around in place this shouldn't throw
        //    // but it will use a constant score rewrite
        //    BooleanQuery.MaxClauseCount =3;

        //    try
        //    {
        //        //Arrange
        //        var sc = _searcher.CreateCriteria("content", SearchCriteria.BooleanOperation.Or);

        //        //set the rewrite method before adding queries
        //        var lsc = (LuceneSearchCriteria)sc;
        //        lsc.QueryParser.MultiTermRewriteMethod = MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE;

        //        sc = sc.NodeName("lo".MultipleCharacterWildcard())
        //            .Or().Field("headerText", "lo".MultipleCharacterWildcard())
        //            .Or().Field("bodyText", "lo".MultipleCharacterWildcard()).Compile();

        //        //Act

        //        Assert.Throws<BooleanQuery.TooManyClauses>(() => _searcher.Search(sc));

        //    }
        //    finally
        //    {
        //        //reset
        //        BooleanQuery.MaxClauseCount = 1024;
        //    }      
        //}


        [Test]
        public void Select_Field()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))

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

                var searcher = indexer.GetSearcher();
                var sc = searcher.CreateQuery("content");
                var sc1 = sc.Field("nodeName", "my name 1").SelectField("__Path");

                var results = sc1.Execute();
                var expectedLoadedFields = new string[] { "__Path" };
                var keys = results.First().Values.Keys.ToArray();
                Assert.True(keys.All(x => expectedLoadedFields.Contains(x)));
                Assert.True(expectedLoadedFields.All(x => keys.Contains(x)));
            }
        }


        [Test]
        public void Select_FirstField()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))

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

                var searcher = indexer.GetSearcher();
                var sc = searcher.CreateQuery("content");
                var sc1 = sc.Field("nodeName", "my name 1").SelectFirstFieldOnly();

                var results = sc1.Execute();
                var expectedLoadedFields = new string[] { "__NodeId" };
                var keys = results.First().Values.Keys.ToArray();
                Assert.True(keys.All(x => expectedLoadedFields.Contains(x)));
                Assert.True(expectedLoadedFields.All(x => keys.Contains(x)));
            }
        }

        [Test]
        public void Select_Fields()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))

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

                var searcher = indexer.GetSearcher();
                var sc = searcher.CreateQuery("content");
                var sc1 = sc.Field("nodeName", "my name 1").SelectFields("nodeName", "bodyText", "id", "__NodeId");

                var results = sc1.Execute();
                var expectedLoadedFields = new string[] { "nodeName", "bodyText", "id", "__NodeId" };
                var keys = results.First().Values.Keys.ToArray();
                Assert.True(keys.All(x => expectedLoadedFields.Contains(x)));
                Assert.True(expectedLoadedFields.All(x => keys.Contains(x)));
            }

        }

        [Test]
        public void Select_Fields_HashSet()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))

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

                var searcher = indexer.GetSearcher();
                var sc = searcher.CreateQuery("content");
                var sc1 = sc.Field("nodeName", "my name 1").SelectFields(new HashSet<string>(new string[] { "nodeName", "bodyText" }));

                var results = sc1.Execute();
                var expectedLoadedFields = new string[] { "nodeName", "bodyText" };
                var keys = results.First().Values.Keys.ToArray();
                Assert.True(keys.All(x => expectedLoadedFields.Contains(x)));
                Assert.True(expectedLoadedFields.All(x => keys.Contains(x)));
            }
        }

        [Test]
        public void Select_Fields_Native_Query()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))

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

                var searcher = indexer.GetSearcher();
                var sc = searcher.CreateQuery().NativeQuery("nodeName:'my name 1'");
                var sc1 = sc.SelectFields("bodyText", "id", "__NodeId");

                var results = sc1.Execute();
                var expectedLoadedFields = new string[] { "bodyText", "id", "__NodeId" };
                var keys = results.First().Values.Keys.ToArray();
                Assert.True(keys.All(x => expectedLoadedFields.Contains(x)));
                Assert.True(expectedLoadedFields.All(x => keys.Contains(x)));
            }

        }

        [Test]
        public void Lucene_Document_Skip_Results_Returns_Different_Results()
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

                var searcher = indexer.GetSearcher();

                //Arrange
                
                var sc = searcher.CreateQuery("content").Field("writerName", "administrator");

                //Act

                var results = (LuceneSearchResults)sc.Execute();
                // this will execute the default search (non skip take)
                var first = results.First();
                // Skip here doesn't re-execute the search since we've already done the default search,
                // it just skips past the lucene documents that we don't want returned as to not allocate
                // more ISearchResult than necessary
                var third = results.Skip(2).First();

                //Assert

                // this will not re-execute since the total item count has already been resolved
                Assert.AreEqual(3, results.TotalItemCount);
                Assert.AreNotEqual(first, third, "Third result should be different");

                // This will re-execute the search as a skiptake search, the previous TopDocs will be replaced.
                // Only the required lucene docs are returned, no need to skip over them, they are not allocated
                // and neither are unnecessary ISearchResult instances.
                var resultsLuceneSkip = results.SkipTake(2); 
                Assert.AreEqual(1, resultsLuceneSkip.Count(), "More results fetched than expected");
                // this will not re-execute since the total item count has already been resolved
                Assert.AreEqual(3, results.TotalItemCount);

                // The search will have executed twice. It will re-execute anytime the search changes from a Default to SkipTake search
                Assert.AreEqual(2, results.ExecutionCount);
            }


        }
    }
}
