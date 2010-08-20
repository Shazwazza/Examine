using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Examine.SearchCriteria;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UmbracoExamine;

namespace Examine.Test
{
    [TestClass]
    public class CriteriaSearchTest
    {
        [TestMethod]
        public void TestMethod1()
        {
        }

        private static IndexInitializer m_Init;
        private static ISearcher m_Searcher;
        private static IIndexer m_Indexer;

        #region Initialize and Cleanup

        [TestInitialize()]
        public void Initialize()
        {
            m_Init = new IndexInitializer();

            m_Searcher = ExamineManager.Instance.SearchProviderCollection["CWSSearcher"];
            m_Indexer = ExamineManager.Instance.IndexProviderCollection["CWSIndexer"];
        }

        [TestMethod]
        public void CriteriaSearch_Cws_TextPage_OrderedByNodeName()
        {
             var criteria = m_Searcher.CreateSearchCriteria(IndexTypes.Content);
            IBooleanOperation query = criteria.NodeTypeAlias("cws_textpage");
             query = query.And().OrderBy("nodeName");
             var sCriteria = query.Compile();
             Console.WriteLine(sCriteria.ToString());
             var results = m_Searcher.Search(sCriteria);

             criteria = m_Searcher.CreateSearchCriteria(IndexTypes.Content);
             IBooleanOperation query2 = criteria.NodeTypeAlias("cws_textpage");
             query2 = query2.And().OrderByDescending("nodeName");
             var sCriteria2 = query2.Compile();
             Console.WriteLine(sCriteria2.ToString());
             var results2 = m_Searcher.Search(sCriteria2);

             Assert.AreNotEqual(results.First().Id, results2.First().Id);
             
        }

        //[ClassCleanup()]
        //public static void Cleanup()
        //{

        //}

        #endregion
    }
}
