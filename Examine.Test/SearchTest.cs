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
        #region Initialize and Cleanup

        private static IndexInit m_Init;

        public static ISearcher Searcher { get; private set; }

        [ClassInitialize()]
        public static void Initialize(TestContext testContext)
        {
            m_Init = new IndexInit();
            Searcher = ExamineManager.Instance.SearchProviderCollection["CWSSearch"];
            ((LuceneExamineSearcher)Searcher).ValidateSearcher(true);
        }

        [ClassCleanup()]
        public static void Cleanup()
        {
            //IndexInit.RemoveWorkingIndex();
        }

        #endregion

        [TestMethod]
        public void TestSimpleSearch()
        {
            var result = Searcher.Search("sam", false);
            Assert.AreEqual(result.Count(), 4, "Results returned for 'sam' should be equal to 4 with the StandardAnalyzer");            
        }

        [TestMethod]
        public void TestSimpleSearchWithWildcard()
        {
            var result = Searcher.Search("umb", true);
            Assert.AreEqual(result.Count(), 7, "Total results for 'umb' is 7 using wildcards");
        }
    }
}
