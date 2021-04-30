//using System;
//using System.IO;
//using System.Text;
//using System.Collections.Generic;
//using System.Linq;

//using Lucene.Net.Search;
//using Examine.Lucene.Providers;
//using Lucene.Net.Index;
//using Examine.Test.DataServices;
//using Examine.Lucene;
//using Lucene.Net.Analysis;
//using Lucene.Net.Analysis.Standard;
//using Lucene.Net.Store;
//using NUnit.Framework;

//namespace Examine.Test
//{
//    [TestFixture]
//    public class SimpleDataProviderTest
//    {
//        [Test]
//        public void SimpleData_RebuildIndex()
//        {
//            var analyzer = new StandardAnalyzer(Util.Version);
//            using (var luceneDir = new RAMDirectory())
//            using (var indexer = GetSimpleIndexer(luceneDir, analyzer, new TestSimpleDataProvider()))
//            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))

//            {
//                indexer.RebuildIndex();

//                session.WaitForChanges();

//                var sc = indexer.SearcherContext;
//                using (var s = sc.GetSearcher())
//                {
//                    var r = s.Searcher.IndexReader;

//                    //there's 7 fields in the index, but 1 sorted fields, 2 are special fields
//                    var fields = r.GetFieldNames(IndexReader.FieldOption.ALL);

//                    Assert.AreEqual(27, fields.Count());

//                    //there should be 5 documents (2 Documents, 3 Pictures)
//                    Assert.AreEqual(5, r.NumDocs());

//                    //test for the special fields to ensure they are there:
//                    Assert.AreEqual(1, fields.Count(x => x == LuceneIndexer.IndexNodeIdFieldName));
//                    Assert.AreEqual(1, fields.Count(x => x == LuceneIndexer.IndexTypeFieldName));
//                }
//            }
//        }

//        [Test]
//        public void SimpleData_Reindex_Node()
//        {
//            var dataProvider = new TestSimpleDataProvider();
//            var analyzer = new StandardAnalyzer(Util.Version);
//            using (var luceneDir = new RAMDirectory())
//            using (var indexer = GetSimpleIndexer(luceneDir, analyzer, dataProvider))
//            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))

//            {
//                indexer.RebuildIndex();

//                session.WaitForChanges();

//                //now we'll index one new node:

//                var dataSet = dataProvider.CreateNewDocument();
//                var xml = dataSet.RowData.ToExamineXml(dataSet.NodeDefinition.NodeId, dataSet.NodeDefinition.Type);

//                indexer.ReIndexNode(xml, "Documents");

//                session.WaitForChanges();

//                var sc = indexer.SearcherContext;
//                using (var s = sc.GetSearcher())
//                {
//                    var r = s.Searcher.IndexReader;

//                    //there should be 6 documents now (3 Documents, 3 Pictures)
//                    Assert.AreEqual(6, r.NumDocs());
//                }
//            }

//        }

//        [Test]
//        public void SimpleDataProviderTest_Range_Search_On_Year()
//        {
//            var dataProvider = new TestSimpleDataProvider();
//            var analyzer = new StandardAnalyzer(Util.Version);
//            using (var luceneDir = new RAMDirectory())
//            using (var indexer = GetSimpleIndexer(luceneDir, analyzer, dataProvider))
//            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))

//            {
//                indexer.RebuildIndex();

//                session.WaitForChanges();

//                var searcher = new LuceneSearcher("testSearcher", luceneDir, analyzer);

//                var query = searcher.CreateSearchCriteria().Range("YearCreated", DateTime.Now.AddYears(-1), DateTime.Now, true, true, SearchCriteria.DateResolution.Year).Compile();
//                var results = searcher.Search(query);

//                Assert.AreEqual(5, results.TotalItemCount);
//            }
//        }

//        public SimpleDataIndexer GetSimpleIndexer(Lucene.Net.Store.Directory luceneDir, Analyzer analyzer, ISimpleDataService dataService)
//        {
//            var i = new SimpleDataIndexer(new IndexCriteria(
//                                                         new IIndexField[] { },
//                                                         new[]
//                                                             {
//                                                                 new TestIndexField ("Author"),
//                                                                 new TestIndexField ("DateCreated", "DateTime"),
//                                                                 new TestIndexField ("Title" ),
//                                                                 new TestIndexField ("Photographer" ),
//                                                                 new TestIndexField ("YearCreated", "Date.Year" ),
//                                                                 new TestIndexField ("MonthCreated", "Date.Month"),
//                                                                 new TestIndexField ("DayCreated", "Date.Day" ),
//                                                                 new TestIndexField ("HourCreated", "Date.Hour" ),
//                                                                 new TestIndexField ("MinuteCreated", "Date.Minute" ),
//                                                                 new TestIndexField ("SomeNumber", "Number" ),
//                                                                 new TestIndexField ("SomeFloat", "Float" ),
//                                                                 new TestIndexField ("SomeDouble", "Double" ),
//                                                                 new TestIndexField ("SomeLong", "Long" )
//                                                             },
//                                                         new string[] { },
//                                                         new string[] { },
//                                                         -1),
//                                                         luceneDir,
//                                                         analyzer,
//                                                         dataService,
//                                                         new[] { "Documents", "Pictures" });
//            i.IndexingError += IndexingError;

//            return i;
//        }

//        internal void IndexingError(object sender, IndexingErrorEventArgs e)
//        {
//            throw e.Exception;
//        }

//        //private static SimpleDataIndexer _indexer;
//        //private static LuceneSearcher _searcher;
//        //private Lucene.Net.Store.Directory _luceneDir;

//        //[SetUp]
//        //public void TestSetup()
//        //{
//        //    _luceneDir = new RAMDirectory();
//        //    _indexer = IndexInitializer.GetSimpleIndexer(_luceneDir);
//        //    _indexer.RebuildIndex();
//        //    _searcher = IndexInitializer.GetLuceneSearcher(_luceneDir);
//        //}

//        //[TearDown]
//        //public void TestTearDown()
//        //{
//        //    _luceneDir.Dispose();
//        //}
//    }
//}
