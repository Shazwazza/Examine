using System.IO;
using Examine.LuceneEngine.Providers;
using UmbracoExamine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml.XPath;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace Examine.Test
{
    /// <summary>
    ///This is a test class for XsltExtensionsTest and is intended
    ///to contain all XsltExtensionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class XsltExtensionsTest
    {
        
        private static LuceneSearcher _searcher;
        private static IIndexer _indexer;

        #region Initialize and Cleanup

        [ClassInitialize()]
        public static void Initialize(TestContext context)
        {
            var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
            _indexer = IndexInitializer.GetUmbracoIndexer(newIndexFolder);
            _indexer.RebuildIndex();
            _searcher = IndexInitializer.GetUmbracoSearcher(newIndexFolder);
        }

        #endregion

        /// <summary>
        ///A test for Search
        ///</summary>
        [TestMethod()]
        public void XSLTSearch_No_Wildcards()
        {
            var result = XsltExtensions.Search("sam", false, _searcher, string.Empty);
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual(4, result.Current.Select("//node").Count, "Results returned for 'sam' should be equal to 5 with the StandardAnalyzer");            
        }

        [TestMethod()]
        public void XSLTSearch_With_Wildcards()
        {
            var result = XsltExtensions.Search("umb", true, _searcher, string.Empty);
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual<int>(7, result.Current.Select("//node").Count, "Total results for 'umb' is 8 using wildcards");
        }  

        /// <summary>
        ///A test for SearchContentOnly
        ///</summary>
        [TestMethod()]
        public void XSLTSearch_Content_Only_No_Wildcards()
        {
            var result = XsltExtensions.Search("sam", false, _searcher, IndexTypes.Content);
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual<int>(3, result.Current.Select("//node").Count, "Total results for 'sam' is 4 using wildcards");
        }

        [TestMethod()]
        public void XSLTSearch_Content_Only_With_Wildcards()
        {
            var result = XsltExtensions.Search("umb", true, _searcher, IndexTypes.Content);
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual<int>(6, result.Current.Select("//node").Count, "Total results for 'umb' is 7 using wildcards");
        }

        /// <summary>
        ///A test for SearchMediaOnly
        ///</summary>
        [TestMethod()]
        public void XSLTSearch_Media_With_And_Without_Wildcards()
        {
            var result = XsltExtensions.Search("umb", true, _searcher, IndexTypes.Media);
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual<int>(1, result.Current.Select("//node").Count, "Total results for 'umb' is 1 using wildcards");

            result = XsltExtensions.Search("umb", false, _searcher, IndexTypes.Media);
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual<int>(0, result.Current.Select("//node").Count, "Total results for 'umb' is 0 without wildcards");
        }

        /// <summary>
        ///A test for SearchMemberOnly
        ///</summary>
        [TestMethod()]
        public void XSLTSearch_Member_Only_No_Wildcards()
        {
            var result = XsltExtensions.Search("mem", false, _searcher, IndexTypes.Member);
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual<int>(0, result.Current.Select("//node").Count, "Total results for 'mem' is 0 using wildcards");
        }

        [TestMethod()]
        public void XSLTSearch_Member_Only_With_Wildcards()
        {
            var result = XsltExtensions.Search("mem", true, _searcher, IndexTypes.Member);
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual<int>(0, result.Current.Select("//node").Count, "Total results for 'mem' is 0 using wildcards");
        }
    }
}
