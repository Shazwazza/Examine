using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UmbracoExamine.Config;

namespace Examine.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class SearchTest
    {

        #region Initialize and Cleanup

        private static IndexInit m_Init = new IndexInit("SearchWorkingTest");
        
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            m_Init.RemoveWorkingIndex();

            var d = m_Init.CreateFromTemplate();
            m_Init.UpdateIndexPaths();
        }

        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            //IndexInit.RemoveWorkingIndex();
        } 

        #endregion

        [TestMethod]
        public void TestSimpleSearch()
        {
            var result = ExamineManager.Instance.Search("sam", false);
            Assert.AreEqual(result.Count(), 4, "Results returned for 'sam' should be equal to 4 with the StandardAnalyzer");            
        }

        [TestMethod]
        public void TestSimpleSearchWithWildcard()
        {
            var result = ExamineManager.Instance.Search("umb", true);
            Assert.AreEqual(result.Count(), 7, "Total results for 'umb' is 7 using wildcards");
        }



    }
}
