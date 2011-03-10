using System;
using System.IO;
using System.Linq;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Lucene.Net.Analysis.Standard;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UmbracoExamine;
using Lucene.Net.Search;
using Lucene.Net.Index;
using System.Diagnostics;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using System.Threading;
using Examine.Test.DataServices;

namespace Examine.Test.Search
{
    [TestClass]
    public class FluentApiTests
    {

        [TestMethod]
        public void FluentApi_Search_Raw_Query()
        {
            var criteria = _searcher.CreateSearchCriteria(IndexTypes.Content);
            var filter = criteria.RawQuery("nodeTypeAlias:CWS_Home");

            var results = _searcher.Search(filter);

            Assert.IsTrue(results.TotalItemCount > 0);
        }

        [TestMethod]
        public void FluentApi_Split_Search_Term()
        {
            var searchTerm = "Billy Bob";

            var criteria = _searcher.CreateSearchCriteria();            
            IQuery qry = qry = criteria.GroupedOr(new[] { "PageTitle", "PageContent", "nodeName" }, searchTerm).Or();
            foreach (var t in searchTerm.Split(' '))
            {

                qry.GroupedOr(new[] { "PageTitle", "PageContent", "nodeName" }, t).Or();
            }

            //var contentCriteria = m_Searcher.CreateSearchCriteria(IndexTypes.Content);

            var sdaf = qry.Field(UmbracoContentIndexer.IndexTypeFieldName, IndexTypes.Content).Compile();

            var results = _searcher.Search(sdaf);

            //Assert.IsTrue(results.TotalItemCount > 0);
        }

        [TestMethod]
        public void FluentApi_Find_By_NodeTypeAlias()
        {
            var criteria = _searcher.CreateSearchCriteria(IndexTypes.Content);
            var filter = criteria.NodeTypeAlias("CWS_Home").Compile();

            var results = _searcher.Search(filter);

            Assert.IsTrue(results.TotalItemCount > 0);
        }

        [TestMethod]
        public void FluentApi_Find_Only_Image_Media()
        {

            var criteria = _searcher.CreateSearchCriteria(IndexTypes.Media);
            var filter = criteria.NodeTypeAlias("image").Compile();

            var results = _searcher.Search(filter);

            Assert.IsTrue(results.TotalItemCount > 0);

        }

        [TestMethod]
        public void FluentApi_Find_Both_Media_And_Content()
        {          
            var criteria = _searcher.CreateSearchCriteria(BooleanOperation.Or);
            var filter = criteria
                .Field(UmbracoContentIndexer.IndexTypeFieldName, IndexTypes.Media)
                .Or()
                .Field(UmbracoContentIndexer.IndexTypeFieldName, IndexTypes.Content)
                .Compile();

            var results = _searcher.Search(filter);

            Assert.AreEqual<int>(11, results.Count());

        }

        [TestMethod]
        public void FluentApi_Sort_Result_By_Single_Field()
        {
            var sc = _searcher.CreateSearchCriteria(IndexTypes.Content);
            var sc1 = sc.Field("writerName", "administrator").And().OrderBy("nodeName").Compile();

            sc = _searcher.CreateSearchCriteria(IndexTypes.Content);
            var sc2 = sc.Field("writerName", "administrator").And().OrderByDescending("nodeName").Compile();

            var results1 = _searcher.Search(sc1);
            var results2 = _searcher.Search(sc2);

            Assert.AreNotEqual(results1.First().Id, results2.First().Id);
        }

        [TestMethod]
        public void FluentApi_Standard_Results_Sorted_By_Score()
        {
            //Arrange
            var sc = _searcher.CreateSearchCriteria(IndexTypes.Content, SearchCriteria.BooleanOperation.Or);
            sc = sc.NodeName("umbraco").Or().Field("headerText", "umbraco").Or().Field("bodyText", "umbraco").Compile();

            //Act
            var results = _searcher.Search(sc);

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
            var sc = _searcher.CreateSearchCriteria(IndexTypes.Content);
            sc = sc.Field("writerName", "administrator").Compile();

            //Act
            var results = _searcher.Search(sc);

            //Assert
            Assert.AreNotEqual(results.First(), results.Skip(2).First(), "Third result should be different");
        }

        [TestMethod]
        public void FluentApiTests_Escaping_Includes_All_Words()
        {
            //Arrange
            var sc = _searcher.CreateSearchCriteria(IndexTypes.Content);
            var op = sc.NodeName("codegarden 09".Escape());
            sc = op.Compile();

            //Act
            var results = _searcher.Search(sc);

            //Assert
            Assert.IsTrue(results.TotalItemCount > 0);
        }

        [TestMethod]
        public void FluentApiTests_Grouped_And_Examiness()
        {
            ////Arrange
            var criteria = _searcher.CreateSearchCriteria(IndexTypes.Content);

            //get all node type aliases starting with CWS and all nodees starting with "A"
            var filter = criteria.GroupedAnd(
                new string[] { "nodeTypeAlias", "nodeName" },
                new IExamineValue[] { "CWS".MultipleCharacterWildcard(), "A".MultipleCharacterWildcard() })
                .Compile();


            ////Act
            var results = _searcher.Search(filter);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
        }

        [TestMethod]
        public void FluentApiTests_Examiness_Proximity()
        {
            ////Arrange
            var criteria = _searcher.CreateSearchCriteria(IndexTypes.Content);

            //get all nodes that contain the words warren and creative within 5 words of each other
            var filter = criteria.Field("metaKeywords", "Warren creative".Proximity(5)).Compile();

            ////Act
            var results = _searcher.Search(filter);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
        }

        [TestMethod]
        public void FluentApiTests_Grouped_Or_Examiness()
        {
            ////Arrange
            var criteria = _searcher.CreateSearchCriteria(IndexTypes.Content);

            //get all node type aliases starting with CWS_Home OR and all nodees starting with "About"
            var filter = criteria.GroupedOr(
                new[] { "nodeTypeAlias", "nodeName" },
                new[] { "CWS\\_Home".Boost(10), "About".MultipleCharacterWildcard() })
                .Compile();


            ////Act
            var results = _searcher.Search(filter);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
        }

        [TestMethod]
        public void FluentApiTests_Cws_TextPage_OrderedByNodeName()
        {
            var criteria = _searcher.CreateSearchCriteria(IndexTypes.Content);
            IBooleanOperation query = criteria.NodeTypeAlias("cws_textpage");
            query = query.And().OrderBy("nodeName");
            var sCriteria = query.Compile();
            Console.WriteLine(sCriteria.ToString());
            var results = _searcher.Search(sCriteria);

            criteria = _searcher.CreateSearchCriteria(IndexTypes.Content);
            IBooleanOperation query2 = criteria.NodeTypeAlias("cws_textpage");
            query2 = query2.And().OrderByDescending("nodeName");
            var sCriteria2 = query2.Compile();
            Console.WriteLine(sCriteria2.ToString());
            var results2 = _searcher.Search(sCriteria2);

            Assert.AreNotEqual(results.First().Id, results2.First().Id);

        }

        private static ISearcher _searcher;
        private static IIndexer _indexer;

        #region Initialize and Cleanup

        

        [TestInitialize()]
        public void Initialize()
        {
            var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
            _indexer = IndexInitializer.GetUmbracoIndexer(newIndexFolder);
            _indexer.RebuildIndex();
            _searcher = IndexInitializer.GetUmbracoSearcher(newIndexFolder);
        }

        

        #endregion
    }
}
