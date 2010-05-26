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
        public void TestSimpleSearch()
        {
            var result = m_Searcher.Search("sam", false);
            Assert.AreEqual(result.Count(), 4, "Results returned for 'sam' should be equal to 4 with the StandardAnalyzer");            
        }

        [TestMethod]
        public void TestSimpleSearchWithWildcard()
        {
            var result = m_Searcher.Search("umb", true);
            Assert.AreEqual(result.Count(), 7, "Total results for 'umb' is 7 using wildcards");
        }

        private static IndexInitializer m_Init;
        private static ISearcher m_Searcher;

        #region Initialize and Cleanup

        [TestInitialize()]       
        public void Initialize()
        {
            m_Init = new IndexInitializer();
            m_Searcher = ExamineManager.Instance.SearchProviderCollection["CWSSearch"];
        }

        //[ClassCleanup()]
        //public static void Cleanup()
        //{
            
        //}

        #endregion
    }
}
