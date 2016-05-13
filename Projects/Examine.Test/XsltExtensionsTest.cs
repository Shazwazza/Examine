using System.IO;
using Examine.LuceneEngine.Providers;
using System;
using System.Xml;
using System.Linq;
using Examine.Test.PartialTrust;
using Lucene.Net.Store;
using NUnit.Framework;
using UmbracoExamine;

namespace Examine.Test
{
    /// <summary>
    ///This is a test class for XsltExtensionsTest and is intended
    ///to contain all XsltExtensionsTest Unit Tests
    ///</summary>
    [TestFixture]
	public class XsltExtensionsTest //: AbstractPartialTrustFixture<XsltExtensionsTest>
    {
        
        private static LuceneSearcher _searcher;
        private static IIndexer _indexer;
		private Lucene.Net.Store.Directory _luceneDir;

        #region Initialize and Cleanup

        [TestFixtureSetUp]
        public void TestSetup()
        {
			_luceneDir = new RAMDirectory();
            _indexer = IndexInitializer.GetUmbracoIndexer(_luceneDir);
            _indexer.RebuildIndex();
			_searcher = IndexInitializer.GetUmbracoSearcher(_luceneDir);
        }

        [TestFixtureTearDown]
        public void TestTearDown()
		{
			_luceneDir.Dispose();
		}


        #endregion

        /// <summary>
        ///A test for Search
        ///</summary>
        [Test]
        public void XSLTSearch_No_Wildcards()
        {
            var result = XsltExtensions.Search("sam", false, _searcher, string.Empty);
            Assert.AreEqual(true, result.MoveNext());
            Assert.AreEqual(4, result.Current.Select("//node").Count, "Results returned for 'sam' should be equal to 5 with the StandardAnalyzer");            
        }

        [Test]
        public void XSLTSearch_With_Wildcards()
        {
            var result = XsltExtensions.Search("umb", true, _searcher, string.Empty);
            Assert.AreEqual(true, result.MoveNext());
            Assert.AreEqual(7, result.Current.Select("//node").Count, "Total results for 'umb' is 8 using wildcards");
        }  

        /// <summary>
        ///A test for SearchContentOnly
        ///</summary>
        [Test]
        public void XSLTSearch_Content_Only_No_Wildcards()
        {
            var result = XsltExtensions.Search("sam", false, _searcher, IndexTypes.Content);
            Assert.AreEqual(true, result.MoveNext());
            Assert.AreEqual(3, result.Current.Select("//node").Count, "Total results for 'sam' is 4 using wildcards");
        }

        [Test]
        public void XSLTSearch_Content_Only_With_Wildcards()
        {
            var result = XsltExtensions.Search("umb", true, _searcher, IndexTypes.Content);
            Assert.AreEqual(true, result.MoveNext());
            Assert.AreEqual(6, result.Current.Select("//node").Count, "Total results for 'umb' is 7 using wildcards");
        }

        /// <summary>
        ///A test for SearchMediaOnly
        ///</summary>
        [Test]
        public void XSLTSearch_Media_With_And_Without_Wildcards()
        {
            var result = XsltExtensions.Search("umb", true, _searcher, IndexTypes.Media);
            Assert.AreEqual(true, result.MoveNext());
            Assert.AreEqual(1, result.Current.Select("//node").Count, "Total results for 'umb' is 1 using wildcards");

            result = XsltExtensions.Search("umb", false, _searcher, IndexTypes.Media);
            Assert.AreEqual(true, result.MoveNext());
            Assert.AreEqual(0, result.Current.Select("//node").Count, "Total results for 'umb' is 0 without wildcards");
        }

        /// <summary>
        ///A test for SearchMemberOnly
        ///</summary>
        [Test]
        public void XSLTSearch_Member_Only_No_Wildcards()
        {
            var result = XsltExtensions.Search("mem", false, _searcher, IndexTypes.Member);
            Assert.AreEqual(true, result.MoveNext());
            Assert.AreEqual(0, result.Current.Select("//node").Count, "Total results for 'mem' is 0 using wildcards");
        }

        [Test]
        public void XSLTSearch_Member_Only_With_Wildcards()
        {
            var result = XsltExtensions.Search("mem", true, _searcher, IndexTypes.Member);
            Assert.AreEqual(true, result.MoveNext());
            Assert.AreEqual(0, result.Current.Select("//node").Count, "Total results for 'mem' is 0 using wildcards");
        }

	    
    }
}
