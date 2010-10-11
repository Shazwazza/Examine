using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UmbracoExamine.Config;
using UmbracoExamine;

namespace Examine.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class SearchTest
    {
       
        [TestMethod]
        public void Search_SimpleSearch()
        {
            var result = m_Searcher.Search("sam", false);
            Assert.AreEqual<int>(5, result.Count(), "Results returned for 'sam' should be equal to 5 with the StandardAnalyzer");            
        }

        [TestMethod]
        public void Search_SimpleSearchWithWildcard()
        {
            var result = m_Searcher.Search("umb", true);
            Assert.AreEqual<int>(8, result.Count(), "Total results for 'umb' is 8 using wildcards");
        }

        private static IndexInitializer m_Init;
        private static ISearcher m_Searcher;
        private static IIndexer m_Indexer;

        #region Initialize and Cleanup

        [TestInitialize()]       
        public void Initialize()
        {

            var combinedResults = 
                ExamineManager.Instance.SearchProviderCollection["CWSSearcher"].Search("blah", true)
                .Concat(
                    ExamineManager.Instance.SearchProviderCollection["PDFSearcher"].Search("blah", true));

            m_Init = new IndexInitializer();
            m_Searcher = ExamineManager.Instance.SearchProviderCollection["CWSSearcher"];
            m_Indexer = ExamineManager.Instance.IndexProviderCollection["CWSIndexer"];

            //ensure we're re-indexed before testing
            m_Indexer.RebuildIndex();
        }

        //[ClassCleanup()]
        //public static void Cleanup()
        //{
            
        //}

        #endregion
    }
}
