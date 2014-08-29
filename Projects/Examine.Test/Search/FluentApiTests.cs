using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Indexing.ValueTypes;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Examine.Session;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using NUnit.Framework;
using UmbracoExamine;
using Version = Lucene.Net.Util.Version;

namespace Examine.Test.Search
{
    [TestFixture, RequiresSTA]
    public class FluentApiTests
    {
        [TearDown]
        public void Teardown()
        {            
            DisposableCollector.Clean();
        }

        //TODO: Finish these

        [Test]
        public void Grouped_Or_Examiness()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new
                        {
                            nodeName = "my name 1",
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Home"
                        }),
                    new ValueSet(2, "content",
                        new
                        {
                            nodeName = "About us",
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Test"
                        }),
                    new ValueSet(3, "content",
                        new
                        {
                            nodeName = "my name 3",
                            bodyText = "lorem ipsum",
                            nodeTypeAlias = "CWS_Page"
                        })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                //paths contain punctuation, we'll escape it and ensure an exact match
                var criteria = searcher.CreateSearchCriteria("content");

                //get all node type aliases starting with CWS_Home OR and all nodees starting with "About"
                var filter = criteria.GroupedOr(
                    new[] { "nodeTypeAlias", "nodeName" },
                    new[] { "CWS\\_Home".Boost(10), "About".MultipleCharacterWildcard() });

                var results = searcher.Search(filter.Compile());
                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void Grouped_Or_With_Not()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(

                //TODO: Making this a number makes the query fail - i wonder how to make it work correctly?
                //new[] { new FieldDefinition("umbracoNaviHide", "number") }, 

                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { nodeName = "my name 1", bodyText = "lorem ipsum", headerText = "header 1", umbracoNaviHide = "1" }),
                    new ValueSet(2, "content",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", headerText = "header 2", umbracoNaviHide = "0" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                //paths contain punctuation, we'll escape it and ensure an exact match
                var criteria = searcher.CreateSearchCriteria("content");
                var filter = criteria.GroupedOr(new[] { "nodeName", "bodyText", "headerText" }, "ipsum").Not().Field("umbracoNaviHide", "1");
                var results = searcher.Search(filter.Compile());
                Assert.AreEqual(1, results.TotalItemCount);
            }
        }

        [Test]
        public void Exact_Match_By_Escaped_Path()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);

            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(
                new[] { new FieldDefinition(UmbracoContentIndexer.IndexPathFieldName, "raw") }, 
                luceneDir, analyzer))            
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 1"},
                            {"bodyText", "lorem ipsum"},
                            {UmbracoContentIndexer.IndexPathFieldName, "-1,123,456,789"}
                        }),
                    new ValueSet(2, "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 2"},
                            {"bodyText", "lorem ipsum"},
                            {UmbracoContentIndexer.IndexPathFieldName, "-1,123,456,987"}
                        })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                //paths contain punctuation, we'll escape it and ensure an exact match
                var criteria = searcher.CreateSearchCriteria("content");
                var filter = criteria.Field(UmbracoContentIndexer.IndexPathFieldName, "-1,123,456,789");
                var results1 = searcher.Search(filter.Compile());
                Assert.AreEqual(0, results1.TotalItemCount);

                //now escape it
                var exactcriteria = searcher.CreateSearchCriteria("content");
                var exactfilter = exactcriteria.Field(UmbracoContentIndexer.IndexPathFieldName, "-1,123,456,789".Escape());
                var results2 = searcher.Search(exactfilter.Compile());
                Assert.AreEqual(1, results2.TotalItemCount);
            }


        }

        [Test]
        public void Find_By_ParentId()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(
                new[] { new FieldDefinition("parentID", "number") }, 
                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { nodeName = "my name 1", bodyText = "lorem ipsum", parentID = "1235" }),
                    new ValueSet(2, "content",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", parentID = "1139" }),
                    new ValueSet(3, "content",
                        new { nodeName = "my name 3", bodyText = "lorem ipsum", parentID = "1139" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateSearchCriteria("content");
                var filter = criteria.Field("parentID", 1139);

                var results = searcher.Search(filter.Compile());

                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void Find_By_NodeTypeAlias()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(
                new[] { new FieldDefinition("nodeTypeAlias", "raw") }, 
                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 1"},
                            {"nodeTypeAlias", "CWS_Home"}
                            //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Home"}
                        }),
                    new ValueSet(2, "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 2"},
                            {"nodeTypeAlias", "CWS_Home"}
                            //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Home"}
                        }),
                    new ValueSet(3, "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 3"},
                            {"nodeTypeAlias", "CWS_Page"}
                            //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Page"}
                        })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateSearchCriteria("content");
                var filter = criteria.Field("nodeTypeAlias","CWS_Home".Escape()).Compile();

                var results = searcher.Search(filter);


                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void Search_With_Stop_Words()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { nodeName = "into 1", bodyText = "It was one thing to bring Carmen into it, but Jonathan was another story." }),
                    new ValueSet(2, "content",
                        new { nodeName = "my name 2", bodyText = "Hands shoved backwards into his back pockets, he took slow deliberate steps, as if he had something on his mind."}),
                    new ValueSet(3, "content",
                        new { nodeName = "my name 3", bodyText = "Slowly carrying the full cups into the living room, she handed one to Alex." })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateSearchCriteria();
                var filter = criteria.Field("bodyText", "into")
                    .Or().Field("nodeName", "into");

                var results = searcher.Search(filter.Compile());

                Assert.AreEqual(0, results.TotalItemCount);
            }
        }

        [Test]
        public void Search_Raw_Query()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 1"},
                            {"nodeTypeAlias", "CWS_Home"}
                            //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Home"}
                        }),
                    new ValueSet(2, "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 2"},
                            {"nodeTypeAlias", "CWS_Home"}
                            //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Home"}
                        }),
                    new ValueSet(3, "content",
                        new Dictionary<string, object>
                        {
                            {"nodeName", "my name 3"},
                            {"nodeTypeAlias", "CWS_Page"}
                            //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Page"}
                        })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateSearchCriteria("content");
                var filter = criteria.RawQuery("nodeTypeAlias:CWS_Home");

                var results = searcher.Search(filter);

                Assert.AreEqual(2, results.TotalItemCount);
            }
            
        }


        [Test]
        public void Find_Only_Image_Media()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "media",
                        new { nodeName = "my name 1", bodyText = "lorem ipsum", nodeTypeAlias = "image" }),
                    new ValueSet(2, "media",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", nodeTypeAlias = "image" }),
                    new ValueSet(3, "media",
                        new { nodeName = "my name 3", bodyText = "lorem ipsum", nodeTypeAlias = "file" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateSearchCriteria("media");
                var filter = criteria.Field("nodeTypeAlias","image").Compile();

                var results = searcher.Search(filter);

                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void Find_Both_Media_And_Content()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "media",
                        new { nodeName = "my name 1", bodyText = "lorem ipsum", nodeTypeAlias = "image" }),
                    new ValueSet(2, "media",
                        new { nodeName = "my name 2", bodyText = "lorem ipsum", nodeTypeAlias = "image" }),
                    new ValueSet(3, "content",
                        new { nodeName = "my name 3", bodyText = "lorem ipsum", nodeTypeAlias = "file" }),
                    new ValueSet(4, "other",
                        new { nodeName = "my name 4", bodyText = "lorem ipsum", nodeTypeAlias = "file" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateSearchCriteria(BooleanOperation.Or);
                var filter = criteria
                    .Field(LuceneIndexer.IndexTypeFieldName, "media")
                    .Or()
                    .Field(LuceneIndexer.IndexTypeFieldName, "content")
                    .Compile();

                var results = searcher.Search(filter);

                Assert.AreEqual(3, results.TotalItemCount);
            }
        }

        [Test]
        public void Sort_Result_By_Number_Field()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(
                //Ensure it's set to a number, otherwise it's not sortable
                new[] { new FieldDefinition("sortOrder", "number"), new FieldDefinition("parentID", "number") }, 
                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { nodeName = "my name 1", sortOrder = "3", parentID = "1143" }),
                    new ValueSet(2, "content",
                        new { nodeName = "my name 2", sortOrder = "1", parentID = "1143" }),
                    new ValueSet(3, "content",
                        new { nodeName = "my name 3", sortOrder = "2", parentID = "1143" }),
                    new ValueSet(4, "content",
                        new { nodeName = "my name 4", bodyText = "lorem ipsum", parentID = "2222" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var sc = searcher.CreateSearchCriteria("content");
                var sc1 = sc.Field("parentID", 1143).And().OrderBy(new SortableField("sortOrder", SortType.Int)).Compile();

                var results1 = searcher.Search(sc1).ToArray();

                Assert.AreEqual(3, results1.Length);

                var currSort = 0;
                for (var i = 0; i < results1.Length; i++)
                {
                    Assert.GreaterOrEqual(int.Parse(results1[i].Fields["sortOrder"]), currSort);
                    currSort = int.Parse(results1[i].Fields["sortOrder"]);
                }
            }
        }

        [Test]
        public void Sort_Result_By_Date_Field()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(
                //Ensure it's set to a date, otherwise it's not sortable
                new[] { new FieldDefinition("updateDate", "date"), new FieldDefinition("parentID", "number") },
                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {
                var now = DateTime.Now;

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { nodeName = "my name 1", updateDate = now.AddDays(2).ToString("yyyy-MM-dd"), parentID = "1143" }),
                    new ValueSet(2, "content",
                        new { nodeName = "my name 2", updateDate = now.ToString("yyyy-MM-dd"), parentID = 1143 }),
                    new ValueSet(3, "content",
                        new { nodeName = "my name 3", updateDate = now.AddDays(1).ToString("yyyy-MM-dd"), parentID = 1143 }),
                    new ValueSet(4, "content",
                        new { nodeName = "my name 4", updateDate = now, parentID = "2222" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var sc = searcher.CreateSearchCriteria("content");
                var sc1 = sc.Field("parentID", 1143).And().OrderBy(new SortableField("updateDate", SortType.Double)).Compile();

                var results1 = searcher.Search(sc1).ToArray();

                Assert.AreEqual(3, results1.Length);

                double currSort = 0;
                for (var i = 0; i < results1.Length; i++)
                {
                    Assert.GreaterOrEqual(double.Parse(results1[i].Fields["updateDate"]), currSort);
                    currSort = double.Parse(results1[i].Fields["updateDate"]);
                }
            }
        }

        [Test]
        public void Sort_Result_By_Single_Field()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(
                //Ensure it's set to a fulltextsortable, otherwise it's not sortable
                new[] { new FieldDefinition("nodeName", "fulltextsortable") },
                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { nodeName = "my name 1", writerName = "administrator", parentID = "1143" }),
                    new ValueSet(2, "content",
                        new { nodeName = "my name 2", writerName = "administrator", parentID = "1143" }),
                    new ValueSet(3, "content",
                        new { nodeName = "my name 3", writerName = "administrator", parentID = "1143" }),
                    new ValueSet(4, "content",
                        new { nodeName = "my name 4", writerName = "writer", parentID = "2222" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var sc = searcher.CreateSearchCriteria("content");
                var sc1 = sc.Field("writerName", "administrator").And()
                    .OrderBy(new SortableField("nodeName", SortType.String)).Compile();

                sc = searcher.CreateSearchCriteria("content");
                var sc2 = sc.Field("writerName", "administrator").And()
                    .OrderByDescending(new SortableField("nodeName", SortType.String)).Compile();

                var results1 = searcher.Search(sc1);
                var results2 = searcher.Search(sc2);

                Assert.AreNotEqual(results1.First().LongId, results2.First().LongId);
            }

            
        }

        [Test]
        public void Standard_Results_Sorted_By_Score()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { nodeName = "umbraco", headerText = "world", bodyText = "blah" }),
                    new ValueSet(2, "content",
                        new { nodeName = "umbraco", headerText = "umbraco", bodyText = "blah" }),
                    new ValueSet(3, "content",
                        new { nodeName = "umbraco", headerText = "umbraco", bodyText = "umbraco" }),
                    new ValueSet(4, "content",
                        new { nodeName = "hello", headerText = "world", bodyText = "blah" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var sc = searcher.CreateSearchCriteria("content", BooleanOperation.Or);
                var sc1 = sc.Field("nodeName", "umbraco").Or().Field("headerText", "umbraco").Or().Field("bodyText", "umbraco").Compile();

                var results = searcher.Search(sc1);

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
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { nodeName = "umbraco", headerText = "world", writerName = "administrator" }),
                    new ValueSet(2, "content",
                        new { nodeName = "umbraco", headerText = "umbraco", writerName = "administrator" }),
                    new ValueSet(3, "content",
                        new { nodeName = "umbraco", headerText = "umbraco", writerName = "administrator" }),
                    new ValueSet(4, "content",
                        new { nodeName = "hello", headerText = "world", writerName = "blah" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                //Arrange
                var sc = searcher.CreateSearchCriteria("content");
                sc = sc.Field("writerName", "administrator").Compile();

                //Act
                var results = searcher.Search(sc);

                //Assert
                Assert.AreNotEqual(results.First(), results.Skip(2).First(), "Third result should be different");
            }

            
        }

        [Test]
        public void Escaping_Includes_All_Words()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { nodeName = "codegarden09", headerText = "world", writerName = "administrator" }),
                    new ValueSet(2, "content",
                        new { nodeName = "codegarden 09", headerText = "umbraco", writerName = "administrator" }),
                    new ValueSet(3, "content",
                        new { nodeName = "codegarden  09", headerText = "umbraco", writerName = "administrator" }),
                    new ValueSet(4, "content",
                        new { nodeName = "codegarden 090", headerText = "world", writerName = "blah" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                //Arrange
                var sc = searcher.CreateSearchCriteria("content");
                var op = sc.Field("nodeName", "codegarden 09".Escape());
                sc = op.Compile();

                //Act
                var results = searcher.Search(sc);

                //Assert
                //NOTE: The result is 2 because the double space is removed with the analyzer
                Assert.AreEqual(2, results.TotalItemCount);
            }

            
        }

        [Test]
        public void Grouped_And_Examiness()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { nodeName = "Aloha", nodeTypeAlias = "CWS_Hello"}),
                    new ValueSet(2, "content",
                        new { nodeName = "Helo", nodeTypeAlias = "CWS_World" }),
                    new ValueSet(3, "content",
                        new { nodeName = "Another node", nodeTypeAlias = "SomethingElse" }),
                    new ValueSet(4, "content",
                        new { nodeName = "Always consider this", nodeTypeAlias = "CWS_World" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                //Arrange
                var criteria = searcher.CreateSearchCriteria("content");

                //get all node type aliases starting with CWS and all nodees starting with "A"
                var filter = criteria.GroupedAnd(
                    new[] { "nodeTypeAlias", "nodeName" },
                    new[] { "CWS".MultipleCharacterWildcard(), "A".MultipleCharacterWildcard() })
                    .Compile();


                //Act
                var results = searcher.Search(filter);

                //Assert
                Assert.AreEqual(2, results.TotalItemCount);
            }            
        }

        [Test]
        public void Examiness_Proximity()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { nodeName = "Aloha", metaKeywords = "Warren is likely to be creative" }),
                    new ValueSet(2, "content",
                        new { nodeName = "Helo", metaKeywords = "Creative is Warren's middle name" }),
                    new ValueSet(3, "content",
                        new { nodeName = "Another node", metaKeywords = "If Warren were creative... well, he actually is" }),
                    new ValueSet(4, "content",
                        new { nodeName = "Always consider this", metaKeywords = "Warren is a very talented individual and quite creative" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                //Arrange
                var criteria = searcher.CreateSearchCriteria("content");

                //get all nodes that contain the words warren and creative within 5 words of each other
                var filter = criteria.Field("metaKeywords", "Warren creative".Proximity(5)).Compile();

                //Act
                var results = searcher.Search(filter);

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

            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(
                //Ensure it's set to a float
                new[] { new FieldDefinition("SomeFloat", "float") }, 
                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { nodeName = "Aloha", SomeFloat = 1 }),
                    new ValueSet(2, "content",
                        new { nodeName = "Helo", SomeFloat = 123 }),
                    new ValueSet(3, "content",
                        new { nodeName = "Another node", SomeFloat = 12 }),
                    new ValueSet(4, "content",
                        new { nodeName = "Always consider this", SomeFloat = 25 })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                //all numbers should be between 0 and 100 based on the data source
                var criteria1 = searcher.CreateSearchCriteria();
                var filter1 = criteria1.Range("SomeFloat", 0f, 100f, true, true).Compile();

                var criteria2 = searcher.CreateSearchCriteria();
                var filter2 = criteria2.Range("SomeFloat", 101f, 200f, true, true).Compile();

                //Act
                var results1 = searcher.Search(filter1);
                var results2 = searcher.Search(filter2);

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
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(
                //Ensure it's set to a float
                new[] { new FieldDefinition("SomeNumber", "number") },
                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { nodeName = "Aloha", SomeNumber = 1 }),
                    new ValueSet(2, "content",
                        new { nodeName = "Helo", SomeNumber = 123 }),
                    new ValueSet(3, "content",
                        new { nodeName = "Another node", SomeNumber = 12 }),
                    new ValueSet(4, "content",
                        new { nodeName = "Always consider this", SomeNumber = 25 })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                //all numbers should be between 0 and 100 based on the data source
                var criteria1 = searcher.CreateSearchCriteria();
                var filter1 = criteria1.Range("SomeNumber", 0, 100, true, true).Compile();

                var criteria2 = searcher.CreateSearchCriteria();
                var filter2 = criteria2.Range("SomeNumber", 101, 200, true, true).Compile();

                //Act
                var results1 = searcher.Search(filter1);
                var results2 = searcher.Search(filter2);

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
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(
                //Ensure it's set to a float
                new[] { new FieldDefinition("SomeDouble", "double") },
                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { nodeName = "Aloha", SomeDouble = 1d }),
                    new ValueSet(2, "content",
                        new { nodeName = "Helo", SomeDouble = 123d }),
                    new ValueSet(3, "content",
                        new { nodeName = "Another node", SomeDouble = 12d }),
                    new ValueSet(4, "content",
                        new { nodeName = "Always consider this", SomeDouble = 25d })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                //all numbers should be between 0 and 100 based on the data source
                var criteria1 = searcher.CreateSearchCriteria();
                var filter1 = criteria1.Range("SomeDouble", 0d, 100d, true, true).Compile();

                var criteria2 = searcher.CreateSearchCriteria();
                var filter2 = criteria2.Range("SomeDouble", 101d, 200d, true, true).Compile();

                //Act
                var results1 = searcher.Search(filter1);
                var results2 = searcher.Search(filter2);

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
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(
                //Ensure it's set to a float
                new[] { new FieldDefinition("SomeLong", "long") },
                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { nodeName = "Aloha", SomeLong = 1L }),
                    new ValueSet(2, "content",
                        new { nodeName = "Helo", SomeLong = 123L }),
                    new ValueSet(3, "content",
                        new { nodeName = "Another node", SomeLong = 12L }),
                    new ValueSet(4, "content",
                        new { nodeName = "Always consider this", SomeLong = 25L })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                //all numbers should be between 0 and 100 based on the data source
                var criteria1 = searcher.CreateSearchCriteria();
                var filter1 = criteria1.Range("SomeLong", 0L, 100L, true, true).Compile();

                var criteria2 = searcher.CreateSearchCriteria();
                var filter2 = criteria2.Range("SomeLong", 101L, 200L, true, true).Compile();

                //Act
                var results1 = searcher.Search(filter1);
                var results2 = searcher.Search(filter2);

                //Assert
                Assert.AreEqual(3, results1.TotalItemCount);
                Assert.AreEqual(1, results2.TotalItemCount);
            }
        }

        /// <summary>
        /// test range query with a Date.Minute structure
        /// </summary>
        [Test]
        public void Date_Range_Minute_SimpleIndexSet()
        {
            var reIndexDateTime = DateTime.Now.AddMinutes(-2);

            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(
                
                new[] { new FieldDefinition("MinuteCreated", "date.minute") },
                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { MinuteCreated = reIndexDateTime }),
                    new ValueSet(2, "content",
                        new { MinuteCreated = reIndexDateTime }),
                    new ValueSet(3, "content",
                        new { MinuteCreated = reIndexDateTime.AddMinutes(-10) }),
                    new ValueSet(4, "content",
                        new { MinuteCreated = reIndexDateTime })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateSearchCriteria();
                var filter = criteria.Range("MinuteCreated", 
                    reIndexDateTime, DateTime.Now, true, true, DateResolution.Minute).Compile();

                var criteria2 = searcher.CreateSearchCriteria();
                var filter2 = criteria2.Range("MinuteCreated",
                    reIndexDateTime.AddMinutes(-20), reIndexDateTime.AddMinutes(-1), true, true, DateResolution.Minute).Compile();

                ////Act
                var results = searcher.Search(filter);
                var results2 = searcher.Search(filter2);

                ////Assert
                Assert.AreEqual(3, results.TotalItemCount);
                Assert.AreEqual(1, results2.TotalItemCount);
            }
            
        }

        /// <summary>
        /// test range query with a Date.Hour structure
        /// </summary>
        [Test]
        public void Date_Range_Hour_SimpleIndexSet()
        {
            var reIndexDateTime = DateTime.Now.AddHours(-2);

            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(

                new[] { new FieldDefinition("HourCreated", "date.hour") },
                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { HourCreated = reIndexDateTime }),
                    new ValueSet(2, "content",
                        new { HourCreated = reIndexDateTime }),
                    new ValueSet(3, "content",
                        new { HourCreated = reIndexDateTime.AddHours(-10) }),
                    new ValueSet(4, "content",
                        new { HourCreated = reIndexDateTime })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateSearchCriteria();
                var filter = criteria.Range("HourCreated", reIndexDateTime, DateTime.Now, true, true, DateResolution.Hour).Compile();

                var criteria2 = searcher.CreateSearchCriteria();
                var filter2 = criteria2.Range("HourCreated", reIndexDateTime.AddHours(-20), reIndexDateTime.AddHours(-3), true, true, DateResolution.Hour).Compile();

                ////Act
                var results = searcher.Search(filter);
                var results2 = searcher.Search(filter2);

                ////Assert
                Assert.AreEqual(3, results.TotalItemCount);
                Assert.AreEqual(1, results2.TotalItemCount);
            }
        }

        /// <summary>
        /// test range query with a Date.Day structure
        /// </summary>
        [Test]
        public void Date_Range_Day_SimpleIndexSet()
        {
            var reIndexDateTime = DateTime.Now.AddDays(-2);

            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(

                new[] { new FieldDefinition("DayCreated", "date.day") },
                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { DayCreated = reIndexDateTime }),
                    new ValueSet(2, "content",
                        new { DayCreated = reIndexDateTime }),
                    new ValueSet(3, "content",
                        new { DayCreated = reIndexDateTime.AddDays(-10) }),
                    new ValueSet(4, "content",
                        new { DayCreated = reIndexDateTime })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateSearchCriteria();
                var filter = criteria.Range("DayCreated", reIndexDateTime, DateTime.Now, true, true, DateResolution.Day).Compile();

                var criteria2 = searcher.CreateSearchCriteria();
                var filter2 = criteria2.Range("DayCreated", reIndexDateTime.AddDays(-20), reIndexDateTime.AddDays(-3), true, true, DateResolution.Day).Compile();

                ////Act
                var results = searcher.Search(filter);
                var results2 = searcher.Search(filter2);

                ////Assert
                Assert.AreEqual(3, results.TotalItemCount);
                Assert.AreEqual(1, results2.TotalItemCount);
            }

        }

        /// <summary>
        /// test range query with a Date.Month structure
        /// </summary>
        [Test]
        public void Date_Range_Month_SimpleIndexSet()
        {
            var reIndexDateTime = DateTime.Now.AddMonths(-2);

            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(

                new[] { new FieldDefinition("MonthCreated", "date.month") },
                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { MonthCreated = reIndexDateTime }),
                    new ValueSet(2, "content",
                        new { MonthCreated = reIndexDateTime }),
                    new ValueSet(3, "content",
                        new { MonthCreated = reIndexDateTime.AddMonths(-10) }),
                    new ValueSet(4, "content",
                        new { MonthCreated = reIndexDateTime })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateSearchCriteria();
                var filter = criteria.Range("MonthCreated", reIndexDateTime, DateTime.Now, true, true, DateResolution.Month).Compile();

                var criteria2 = searcher.CreateSearchCriteria();
                var filter2 = criteria2.Range("MonthCreated", reIndexDateTime.AddMonths(-20), reIndexDateTime.AddMonths(-3), true, true, DateResolution.Month).Compile();

                ////Act
                var results = searcher.Search(filter);
                var results2 = searcher.Search(filter2);

                ////Assert
                Assert.AreEqual(3, results.TotalItemCount);
                Assert.AreEqual(1, results2.TotalItemCount);
            }

            
        }

        /// <summary>
        /// test range query with a Date.Year structure
        /// </summary>
        [Test]
        public void Date_Range_Year_SimpleIndexSet()
        {
            var reIndexDateTime = DateTime.Now.AddYears(-2);

            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(

                new[] { new FieldDefinition("YearCreated", "date.year") },
                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { YearCreated = reIndexDateTime }),
                    new ValueSet(2, "content",
                        new { YearCreated = reIndexDateTime }),
                    new ValueSet(3, "content",
                        new { YearCreated = reIndexDateTime.AddMonths(-10) }),
                    new ValueSet(4, "content",
                        new { YearCreated = reIndexDateTime })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateSearchCriteria();

                var filter = criteria.Range("YearCreated", reIndexDateTime, DateTime.Now, true, true, DateResolution.Year).Compile();

                var criteria2 = searcher.CreateSearchCriteria();
                var filter2 = criteria2.Range("YearCreated", DateTime.Now.AddYears(-20), DateTime.Now.AddYears(-3), true, true, DateResolution.Year).Compile();

                ////Act
                var results = searcher.Search(filter);
                var results2 = searcher.Search(filter2);

                ////Assert
                Assert.AreEqual(3, results.TotalItemCount);
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

            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(

                new[] { new FieldDefinition("DateCreated", "datetime") },
                luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { DateCreated = reIndexDateTime }),
                    new ValueSet(2, "content",
                        new { DateCreated = reIndexDateTime }),
                    new ValueSet(3, "content",
                        new { DateCreated = reIndexDateTime.AddMonths(-10) }),
                    new ValueSet(4, "content",
                        new { DateCreated = reIndexDateTime })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateSearchCriteria();
                var filter = criteria.Range("DateCreated", reIndexDateTime, DateTime.Now, true, true).Compile();

                var criteria2 = searcher.CreateSearchCriteria();
                var filter2 = criteria2.Range("DateCreated", reIndexDateTime.AddDays(-1), reIndexDateTime.AddSeconds(-1), true, true).Compile();

                ////Act
                var results = searcher.Search(filter);
                var results2 = searcher.Search(filter2);

                ////Assert
                Assert.IsTrue(results.TotalItemCount > 0);
                Assert.IsTrue(results2.TotalItemCount == 0);
            }

            
        }

        [Test]
        public void Fuzzy_Search()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { Content = "I'm thinking here" }),
                    new ValueSet(2, "content",
                        new { Content = "I'm a thinker" }),
                    new ValueSet(3, "content",
                        new { Content = "I am pretty thoughtful" }),
                    new ValueSet(4, "content",
                        new { Content = "I thought you were cool" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateSearchCriteria();
                var filter = criteria.Field("Content", "think".Fuzzy(0.1F)).Compile();

                var criteria2 = searcher.CreateSearchCriteria();
                var filter2 = criteria2.Field("Content", "thought".Fuzzy()).Compile();

                ////Act
                var results = searcher.Search(filter);
                var results2 = searcher.Search(filter2);

                ////Assert
                Assert.AreEqual(2, results.TotalItemCount);
                Assert.AreEqual(2, results2.TotalItemCount);
                
            }


        }

        [Test]
        public void Max_Count()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {

                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { Content = "hello world" }),
                    new ValueSet(2, "content",
                        new { Content = "hello worlds" }),
                    new ValueSet(3, "content",
                        new { Content = "hello you cruel world" }),
                    new ValueSet(4, "content",
                        new { Content = "hi there, hello world" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateSearchCriteria();
                var filter = criteria.Field("Content", "hello")
                    .Compile()
                    .MaxCount(3);

                ////Act
                var results = searcher.Search(filter);

                ////Assert
                Assert.AreEqual(3, results.TotalItemCount);

            }


        }

        //TODO: Look into LuceneSearchExtensions!!!! Lots of stuff to test there
    }
}
