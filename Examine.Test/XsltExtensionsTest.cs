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

        private static ISearcher m_Searcher;
        private static IIndexer m_Indexer;

        #region Initialize and Cleanup

        [TestInitialize()]
        public void Initialize()
        {
            IndexInitializer.Initialize();
            m_Searcher = ExamineManager.Instance.SearchProviderCollection["CWSSearcher"];
            m_Indexer = ExamineManager.Instance.IndexProviderCollection["CWSIndexer"];

            //ensure we're re-indexed before testing
            m_Indexer.RebuildIndex();
        }

        #endregion

        /// <summary>
        ///A test for Search
        ///</summary>
        [TestMethod()]
        public void XSLTSearch_No_Wildcards()
        {
            var result = XsltExtensions.Search("sam", false, "CWSSearcher");
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual(5, result.Current.Select("//node").Count, "Results returned for 'sam' should be equal to 5 with the StandardAnalyzer");            
        }

        [TestMethod()]
        public void XSLTSearch_With_Wildcards()
        {   
            var result = XsltExtensions.Search("umb", true, "CWSSearcher");
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual<int>(8, result.Current.Select("//node").Count, "Total results for 'umb' is 8 using wildcards");
        }  

        /// <summary>
        ///A test for SearchContentOnly
        ///</summary>
        [TestMethod()]
        public void XSLTSearch_Content_Only_No_Wildcards()
        {
            var result = XsltExtensions.SearchContentOnly("sam", false, "CWSSearcher");
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual<int>(4, result.Current.Select("//node").Count, "Total results for 'sam' is 4 using wildcards");
        }

        [TestMethod()]
        public void XSLTSearch_Content_Only_With_Wildcards()
        {
            var result = XsltExtensions.SearchContentOnly("umb", true, "CWSSearcher");
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual<int>(7, result.Current.Select("//node").Count, "Total results for 'umb' is 7 using wildcards");
        }

        /// <summary>
        ///A test for SearchMediaOnly
        ///</summary>
        [TestMethod()]
        public void XSLTSearch_Media_With_And_Without_Wildcards()
        {
            var result = XsltExtensions.SearchMediaOnly("umb", true, "CWSSearcher");
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual<int>(1, result.Current.Select("//node").Count, "Total results for 'umb' is 1 using wildcards");

            result = XsltExtensions.SearchMediaOnly("umb", false, "CWSSearcher");
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual<int>(0, result.Current.Select("//node").Count, "Total results for 'umb' is 0 without wildcards");
        }

        /// <summary>
        ///A test for SearchMemberOnly
        ///</summary>
        [TestMethod()]
        public void XSLTSearch_Member_Only_No_Wildcards()
        {
            var result = XsltExtensions.SearchMemberOnly("mem", false, "CWSSearcher");
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual<int>(0, result.Current.Select("//node").Count, "Total results for 'mem' is 0 using wildcards");
        }

        [TestMethod()]
        public void XSLTSearch_Member_Only_With_Wildcards()
        {
            var result = XsltExtensions.SearchMemberOnly("mem", true, "CWSSearcher");
            Assert.AreEqual<bool>(true, result.MoveNext());
            Assert.AreEqual<int>(0, result.Current.Select("//node").Count, "Total results for 'mem' is 0 using wildcards");
        }
    }
}
