using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UmbracoExamine;
using Examine.Test.DataServices;

namespace Examine.Test
{
    [TestClass]
    public class ReIndexingTest
    {

        /// <summary>
        /// This test makes sure that .del files get processed before .add files
        /// </summary>
        [TestMethod]
        public void ReIndexing_Ensure_Queue_File_Ordering()
        {
            var isDeleted = false;
            var isAdded = false;
            
            //first we need to wire up some events... we need to ensure that during an update process that the item is deleted before it's added
            var indexer = GetIndexer();
            
            //add index deleted event handler
            indexer.IndexDeleted += (sender, e) =>
            {
                isDeleted = true;
                Assert.IsFalse(isAdded, "node was added before it was deleted!");
            };

            //add index added event handler
            indexer.NodeIndexed += (sender, e) =>
            {
                isAdded = true;
                Assert.IsTrue(isDeleted, "node was not deleted first!");
            };

            //get a node from the data repo
            var node = m_DataService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .First();

            //this will do the reindex (deleting, then updating)
            indexer.ReIndexNode(node, IndexTypes.Content);

            Assert.IsTrue(isDeleted, "node was not deleted");
            Assert.IsTrue(isAdded, "node was not re-added");
        }


        #region Private methods and properties

        private TestContentService m_DataService = new TestContentService();        

        /// <summary>
        /// Helper method to return the index searcher for this index
        /// </summary>
        /// <returns></returns>
        private UmbracoExamineSearcher GetSearcherProvider()
        {
            return (UmbracoExamineSearcher)ExamineManager.Instance.SearchProviderCollection["CWSIndexer"];
        }

        /// <summary>
        /// Helper method to return the indexer that we are testing
        /// </summary>
        /// <returns></returns>
        private UmbracoExamineIndexer GetIndexer()
        {
            return (UmbracoExamineIndexer)ExamineManager.Instance.IndexProviderCollection["CWSIndexer"];
        }

        #endregion

        #region Initialize and Cleanup

        private static IndexInitializer m_Init;

        [TestInitialize()]
        public void Initialize()
        {
            m_Init = new IndexInitializer();
        }

        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{

        //}

        #endregion
    }
}
