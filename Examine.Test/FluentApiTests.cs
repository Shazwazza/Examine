using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UmbracoExamine;
using Examine.SearchCriteria;
using Examine.LuceneEngine.SearchCriteria;
using System.Threading;

namespace Examine.Test
{
    [TestClass]
    public class FluentApiTests
    {

        [TestMethod]
        public void FluentApi_Find_By_NodeTypeAlias()
        {
            var criteria = m_Searcher.CreateSearchCriteria(IndexTypes.Content);
            var filter = criteria.NodeTypeAlias("CWS_Home").Compile();

            var results = m_Searcher.Search(filter);

            Assert.IsTrue(results.TotalItemCount > 0);
        }

        [TestMethod]
        public void FluentApi_Find_Only_Image_Media()
        {

            var criteria = m_Searcher.CreateSearchCriteria(IndexTypes.Media);
            var filter = criteria.NodeTypeAlias("image").Compile();

            var results = m_Searcher.Search(filter);

            Assert.IsTrue(results.TotalItemCount > 0);

        }

        [TestMethod]
        public void FluentApi_Find_Both_Media_And_Content()
        {          
            var criteria = m_Searcher.CreateSearchCriteria(BooleanOperation.Or);
            var filter = criteria
                .Field(UmbracoExamineIndexer.IndexTypeFieldName, IndexTypes.Media)
                .Or()
                .Field(UmbracoExamineIndexer.IndexTypeFieldName, IndexTypes.Content)
                .Compile();

            var results = m_Searcher.Search(filter);

            Assert.AreEqual<int>(11, results.Count());

        }

        [TestMethod]
        public void FluentApi_Sort_Result_By_Single_Field()
        {
            var sc = m_Searcher.CreateSearchCriteria(IndexTypes.Content);
            var sc1 = sc.Field("writerName", "administrator").And().OrderBy("nodeName").Compile();

            sc = m_Searcher.CreateSearchCriteria(IndexTypes.Content);
            var sc2 = sc.Field("writerName", "administrator").And().OrderByDescending("nodeName").Compile();

            var results1 = m_Searcher.Search(sc1);
            var results2 = m_Searcher.Search(sc2);

            Assert.AreNotEqual(results1.First().Id, results2.First().Id);
        }

        [TestMethod]
        public void FluentApi_Standard_Results_Sorted_By_Score()
        {
            //Arrange
            var sc = m_Searcher.CreateSearchCriteria(IndexTypes.Content, SearchCriteria.BooleanOperation.Or);
            sc = sc.NodeName("umbraco").Or().Field("headerText", "umbraco").Or().Field("bodyText", "umbraco").Compile();

            //Act
            var results = m_Searcher.Search(sc);

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
        public void FluentApi_Skip_Results_Returns_Different_Results()
        {
            //Arrange
            var sc = m_Searcher.CreateSearchCriteria(IndexTypes.Content);
            sc = sc.Field("writerName", "administrator").Compile();

            //Act
            var results = m_Searcher.Search(sc);

            //Assert
            Assert.AreNotEqual(results.First(), results.Skip(2).First(), "Third result should be different");
        }

        [TestMethod]
        public void FluentApiTests_Escaping_Includes_All_Words()
        {
            //Arrange
            var sc = m_Searcher.CreateSearchCriteria(IndexTypes.Content);
            var op = sc.NodeName("codegarden 09".Escape());
            sc = op.Compile();

            //Act
            var results = m_Searcher.Search(sc);

            //Assert
            Assert.IsTrue(results.TotalItemCount > 0);
        }

        [TestMethod]
        public void FluentApiTests_Grouped_And_Examiness()
        {
            ////Arrange
            var criteria = m_Searcher.CreateSearchCriteria(IndexTypes.Content);

            //get all node type aliases starting with CWS and all nodees starting with "A"
            var filter = criteria.GroupedAnd(
                new string[] { "nodeTypeAlias", "nodeName" },
                new IExamineValue[] { "CWS".MultipleCharacterWildcard(), "A".MultipleCharacterWildcard() })
                .Compile();


            ////Act
            var results = m_Searcher.Search(filter);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
        }

        [TestMethod]
        public void FluentApiTests_Examiness_Proximity()
        {
            ////Arrange
            var criteria = m_Searcher.CreateSearchCriteria(IndexTypes.Content);

            //get all nodes that contain the words warren and creative within 5 words of each other
            var filter = criteria.Field("metaKeywords", "Warren creative".Proximity(5)).Compile();

            ////Act
            var results = m_Searcher.Search(filter);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
        }

        [TestMethod]
        public void FluentApiTests_Grouped_Or_Examiness()
        {
            ////Arrange
            var criteria = m_Searcher.CreateSearchCriteria(IndexTypes.Content);

            //get all node type aliases starting with CWS_Home OR and all nodees starting with "About"
            var filter = criteria.GroupedOr(
                new string[] { "nodeTypeAlias", "nodeName" },
                new IExamineValue[] { "CWS\\_Home".Boost(10), "About".MultipleCharacterWildcard() })
                .Compile();


            ////Act
            var results = m_Searcher.Search(filter);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
        }

        [TestMethod]
        public void FluentApiTests_Cws_TextPage_OrderedByNodeName()
        {
            //re-index since the demo index is old
            m_Indexer.RebuildIndex();

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

            //ensure everything is rebuilt before testing
            m_Indexer.RebuildIndex();
        }

        //[ClassCleanup()]
        //public static void Cleanup()
        //{

        //}

        #endregion
    }
}
