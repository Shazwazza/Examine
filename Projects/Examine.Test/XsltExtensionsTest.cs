using System;
using System.IO;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Providers;

using System.Xml;
using System.Linq;
using Examine.Session;
using Examine.Test.DataServices;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using NUnit.Framework;
using UmbracoExamine;
using UmbracoExamine.DataServices;
using Version = Lucene.Net.Util.Version;

namespace Examine.Test
{
    /// <summary>
    ///This is a test class for XsltExtensionsTest and is intended
    ///to contain all XsltExtensionsTest Unit Tests
    ///</summary>
    [TestFixture]
    public class XsltExtensionsTest
    {
        [SetUp]
        public void TestSetup()
        {
            //set the flag to disable init check
            BaseUmbracoIndexer.DisableInitializationCheck = true;
        }

        /// <summary>
        ///A test for Search
        ///</summary>
        [Test]
        public void XSLTSearch_No_Wildcards()
        {
            var dataProvider = new TestDataService();
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = GetUmbracoIndexer(luceneDir, analyzer, dataProvider))
            using (SearcherContextCollection.Instance)
            {
                indexer.RebuildIndex();

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var result = XsltExtensions.Search("sam", false, searcher, string.Empty);
                Assert.AreEqual(true, result.MoveNext());
                Assert.AreEqual(5, result.Current.Select("//node").Count, "Results returned for 'sam' should be equal to 5 with the StandardAnalyzer");
            }
        }

        [Test]
        public void XSLTSearch_With_Wildcards()
        {
            var dataProvider = new TestDataService();
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = GetUmbracoIndexer(luceneDir, analyzer, dataProvider))
            using (SearcherContextCollection.Instance)
            {
                indexer.RebuildIndex();

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var result = XsltExtensions.Search("umb", true, searcher, string.Empty);
                Assert.AreEqual(true, result.MoveNext());
                Assert.AreEqual(8, result.Current.Select("//node").Count, "Total results for 'umb' is 8 using wildcards");
            }
        }

        /// <summary>
        ///A test for SearchContentOnly
        ///</summary>
        [Test]
        public void XSLTSearch_Content_Only_No_Wildcards()
        {
            var dataProvider = new TestDataService();
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = GetUmbracoIndexer(luceneDir, analyzer, dataProvider))
            using (SearcherContextCollection.Instance)
            {
                indexer.RebuildIndex();

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var result = XsltExtensions.Search("sam", false, searcher, IndexTypes.Content);
                Assert.AreEqual(true, result.MoveNext());
                Assert.AreEqual(4, result.Current.Select("//node").Count, "Total results for 'sam' is 4 using wildcards");
            }
        }

        [Test]
        public void XSLTSearch_Content_Only_With_Wildcards()
        {
            var dataProvider = new TestDataService();
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = GetUmbracoIndexer(luceneDir, analyzer, dataProvider))
            using (SearcherContextCollection.Instance)
            {
                indexer.RebuildIndex();

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var result = XsltExtensions.Search("umb", true, searcher, IndexTypes.Content);
                Assert.AreEqual(true, result.MoveNext());
                Assert.AreEqual(7, result.Current.Select("//node").Count, "Total results for 'umb' is 7 using wildcards");
            }
        }

        /// <summary>
        ///A test for SearchMediaOnly
        ///</summary>
        [Test]
        public void XSLTSearch_Media_With_And_Without_Wildcards()
        {
            var dataProvider = new TestDataService();
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = GetUmbracoIndexer(luceneDir, analyzer, dataProvider))
            using (SearcherContextCollection.Instance)
            {
                indexer.RebuildIndex();

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var result = XsltExtensions.Search("umb", true, searcher, IndexTypes.Media);
                Assert.AreEqual(true, result.MoveNext());
                Assert.AreEqual(1, result.Current.Select("//node").Count, "Total results for 'umb' is 1 using wildcards");

                result = XsltExtensions.Search("umb", false, searcher, IndexTypes.Media);
                Assert.AreEqual(true, result.MoveNext());
                Assert.AreEqual(0, result.Current.Select("//node").Count, "Total results for 'umb' is 0 without wildcards");
            }
        }

        /// <summary>
        ///A test for SearchMemberOnly
        ///</summary>
        [Test]
        public void XSLTSearch_Member_Only_No_Wildcards()
        {
            var dataProvider = new TestDataService();
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = GetUmbracoIndexer(luceneDir, analyzer, dataProvider))
            using (SearcherContextCollection.Instance)
            {
                indexer.RebuildIndex();

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var result = XsltExtensions.Search("mem", false, searcher, IndexTypes.Member);
                Assert.AreEqual(true, result.MoveNext());
                Assert.AreEqual(0, result.Current.Select("//node").Count, "Total results for 'mem' is 0 using wildcards");
            }
        }

        [Test]
        public void XSLTSearch_Member_Only_With_Wildcards()
        {
            var dataProvider = new TestDataService();
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = GetUmbracoIndexer(luceneDir, analyzer, dataProvider))
            using (SearcherContextCollection.Instance)
            {
                indexer.RebuildIndex();

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var result = XsltExtensions.Search("mem", true, searcher, IndexTypes.Member);
                Assert.AreEqual(true, result.MoveNext());
                Assert.AreEqual(0, result.Current.Select("//node").Count, "Total results for 'mem' is 0 using wildcards");
            }
        }


        public UmbracoContentIndexer GetUmbracoIndexer(Lucene.Net.Store.Directory luceneDir, Analyzer analyzer, IDataService dataService)
        {

            var i = new UmbracoContentIndexer(new IndexCriteria(
                                                         new[]
                                                             {
                                                                 new TestIndexField ("id", "Number", true), 
                                                                 new TestIndexField ("nodeName", true ),
                                                                 new TestIndexField ("updateDate", "DateTime", true), 
                                                                 new TestIndexField ("writerName" ), 
                                                                 new TestIndexField ("path" ), 
                                                                 new TestIndexField ("nodeTypeAlias" ), 
                                                                 new TestIndexField ("parentID" ),
                                                                 new TestIndexField ("sortOrder", "Number", true),
                                                             },
                                                         new[]
                                                             {
                                                                 new TestIndexField ("headerText" ), 
                                                                 new TestIndexField ("bodyText" ),
                                                                 new TestIndexField ("metaDescription" ), 
                                                                 new TestIndexField ("metaKeywords" ), 
                                                                 new TestIndexField ("bodyTextColOne" ), 
                                                                 new TestIndexField ("bodyTextColTwo" ), 
                                                                 new TestIndexField ("xmlStorageTest" ),
                                                                 new TestIndexField ("umbracoNaviHide" )
                                                             },
                                                         new[]
                                                             {
                                                                 "CWS_Home", 
                                                                 "CWS_Textpage",
                                                                 "CWS_TextpageTwoCol", 
                                                                 "CWS_NewsEventsList", 
                                                                 "CWS_NewsItem", 
                                                                 "CWS_Gallery", 
                                                                 "CWS_EventItem", 
                                                                 "Image", 
                                                             },
                                                         new string[] { },
                                                         -1),
                                                         luceneDir,
                                                         dataService,
                                                         analyzer);

            //i.IndexSecondsInterval = 1;

            i.IndexingError += IndexingError;

            return i;
        }

        internal void IndexingError(object sender, IndexingErrorEventArgs e)
        {
            throw new ApplicationException(e.Message, e.InnerException);
        }

    }
}
