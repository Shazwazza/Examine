using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UmbracoExamine;

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
            ((LuceneExamineSearcher)Searcher).ValidateSearcher(true);
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

        [TestMethod]
        public void Standard_Results_Sorted_By_Score()
        {
            //Arrange
            var sc = Searcher.CreateSearchCriteria(IndexType.Content, SearchCriteria.BooleanOperation.Or);
            sc = sc.NodeName("umbraco").Or().Field("headerText", "umbraco").Or().Field("bodyText", "umbraco").Compile();

            //Act
            var results = Searcher.Search(sc);

            //Assert
            for (int i = 0; i < results.TotalItemCount - 1; i++)
            {
                var curr = results.ElementAt(i);
                var next = results.ElementAtOrDefault(i + 1);

                if (next == null)
                    break;

                Assert.IsTrue(curr.Score >= next.Score, string.Format("Result at index {0} must have a higher score than result at index {1}", i, i + 1));
            }
        }

        [TestMethod]
        public void Skip_Results_Returns_Different_Results()
        {
            //Arrange
            var sc = Searcher.CreateSearchCriteria(IndexType.Content);
            sc = sc.Field("writerName", "administrator").Compile();
            
            //Act
            var results = Searcher.Search(sc);

            //Assert
            Assert.AreNotEqual(results.First(), results.Skip(2).First(), "Third result should be different");
        }
    }
}
