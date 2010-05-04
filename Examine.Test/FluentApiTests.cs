using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Examine.Test
{
    [TestClass]
    public class FluentApiTests
    {
        #region Initialize and Cleanup

        private static IndexInit m_Init;

        public static ISearcher Searcher { get; private set; }

        [ClassInitialize()]
        public static void Initialize(TestContext testContext)
        {
            m_Init = new IndexInit();
            Searcher = ExamineManager.Instance.SearchProviderCollection["CWSSearch"];
        }

        [ClassCleanup()]
        public static void Cleanup()
        {
            //IndexInit.RemoveWorkingIndex();
        }

        #endregion

        [TestMethod]
        public void Sort_Result_By_Single_Field()
        {
            var sc = Searcher.CreateSearchCriteria(IndexType.Content);
            var sc1 = sc.Field("writerName", "administrator").And().OrderBy("nodeName").Compile();

            sc = Searcher.CreateSearchCriteria(IndexType.Content);
            var sc2 = sc.Field("writerName", "administrator").And().OrderByDescending("nodeName").Compile();

            var results1 = Searcher.Search(sc1);
            var results2 = Searcher.Search(sc2);

            Assert.AreNotEqual(results1.First().Id, results2.First().Id);
        }
    }
}
