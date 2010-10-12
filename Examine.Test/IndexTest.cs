using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UmbracoExamine;
using Lucene.Net.Search;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store;
using System.Diagnostics;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using System.Threading;
using Examine.Test.DataServices;

namespace Examine.Test
{

    /// <summary>
    /// Tests the standard indexing capabilities
    /// </summary>
    [TestClass]
    public class IndexTest
    {

        //TODO: This will fail because 

        //[TestMethod]
        //public void Index_Rebuild_Convension_Name_Index()
        //{
        //    var indexer = (LuceneExamineIndexer)ExamineManager.Instance.IndexProviderCollection["ConvensionNamedIndexer"];
        //    indexer.RebuildIndex();

        //    //do validation... though I'm not sure how many there should be because this index set is empty so will index everything

        //    //get searcher and reader to get stats
        //    var s = (LuceneExamineSearcher)ExamineManager.Instance.SearchProviderCollection["ConvensionNamedSearcher"];
        //    var r = s.GetSearcher().GetIndexReader();

        //    //there's 15 fields in the index, but 3 sorted fields
        //    var fields = r.GetFieldNames(IndexReader.FieldOption.ALL);
        //}

        /// <summary>
        /// This will create a new index queue item for the same ID multiple times to ensure that the 
        /// index does not end up with duplicate entries.
        /// </summary>
        [TestMethod]
        public void Index_Ensure_No_Duplicates_In_Async()
        {
            var indexer = GetIndexer();

            //add the handler
            var handler = new EventHandler<IndexedNodesEventArgs>(indexer_NodesIndexed);
            indexer.NodesIndexed += handler;

            //run in async mode
            indexer.RunAsync = true;

            //get a node from the data repo
            var node = m_DataService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .First();

            //get the id for th node we're re-indexing.
            var id = (int)node.Attribute("id");

            //set our internal monitoring flag
            m_IsIndexing = true;

            //reindex the same node 210 times
            for (var i = 0; i < 210; i++)
            {
                indexer.ReIndexNode(node, IndexTypes.Content);
            }

            //we need to check if the indexing is complete
            while (m_IsIndexing)
            {
                //wait until indexing is done
                Thread.Sleep(1000);
            }

            //reset the async mode and remove event handler
            indexer.RunAsync = false;
            indexer.NodesIndexed -= handler;

            //ensure no duplicates
            var search = GetSearcherProvider();
            var results = search.Search(search.CreateSearchCriteria().Id(id).Compile());
            Assert.AreEqual<int>(1, results.Count());
        }

        [TestMethod]
        public void Index_Ensure_No_Duplicates_In_Non_Async()
        {
            var indexer = GetIndexer();

            //get a node from the data repo
            var node = m_DataService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .First();

            //get the id for th node we're re-indexing.
            var id = (int)node.Attribute("id");

            //reindex the same node 210 times
            for (var i = 0; i < 210; i++)
            {
                indexer.ReIndexNode(node, IndexTypes.Content);
            }
           
            //ensure no duplicates
            var search = GetSearcherProvider();
            var results = search.Search(search.CreateSearchCriteria().Id(id).Compile());
            Assert.AreEqual<int>(1, results.Count());
        }

        /// <summary>
        /// Used to monitor async operation
        /// </summary>
        private bool m_IsIndexing = false;
        
        /// <summary>
        /// Used to monitor an Async operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void indexer_NodesIndexed(object sender, IndexedNodesEventArgs e)
        {
            m_IsIndexing = false;
        }

        /// <summary>
        /// This test makes sure that .del files get processed before .add files
        /// </summary>
        [TestMethod]
        public void Index_Ensure_Queue_File_Ordering()
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

        [TestMethod]
        public void Index_Rebuild_Index()
        {
            //get searcher and reader to get stats
            var s = GetSearcherProvider();
            var r = s.GetSearcher().GetIndexReader();

            Trace.Write("Num docs = " + r.NumDocs().ToString());


            var indexer = GetIndexer();
            indexer.RebuildIndex();
            
            //do validation...

            //get searcher and reader to get stats
            s = GetSearcherProvider();
            r = s.GetSearcher().GetIndexReader();

            //there's 16 fields in the index, but 3 sorted fields
            var fields = r.GetFieldNames(IndexReader.FieldOption.ALL);

            Assert.AreEqual(21, fields.Count());
            //ensure there's 3 sorting fields
            Assert.AreEqual(3, fields.Where(x => x.StartsWith(UmbracoContentIndexer.SortedFieldNamePrefix)).Count());
            //there should be 11 documents (10 content, 1 media)
            Assert.AreEqual(11, r.NumDocs());

            //test for the special fields to ensure they are there:
            Assert.AreEqual<int>(1, fields.Where(x => x == LuceneIndexer.IndexNodeIdFieldName).Count());
            Assert.AreEqual<int>(1, fields.Where(x => x == LuceneIndexer.IndexTypeFieldName).Count());
            Assert.AreEqual<int>(1, fields.Where(x => x == UmbracoContentIndexer.IndexPathFieldName).Count());
            Assert.AreEqual<int>(1, fields.Where(x => x == UmbracoContentIndexer.NodeTypeAliasFieldName).Count());

        }

        /// <summary>
        /// This will ensure that all 'Content' (not media) is cleared from the index using the Lucene API directly.
        /// We then call the Examine method to re-index Content and do some comparisons to ensure that it worked correctly.
        /// </summary>
        [TestMethod]
        public void Index_Reindex_Content()
        {
            var searcher = GetSearcherProvider();

            Trace.WriteLine("Searcher folder is " + searcher.LuceneIndexFolder.FullName);
            var s = searcher.GetSearcher();

            //first delete all 'Content' (not media). This is done by directly manipulating the index with the Lucene API, not examine!
            var r = IndexReader.Open(s.GetIndexReader().Directory(), false);            
            var contentTerm = new Term(UmbracoContentIndexer.IndexTypeFieldName, IndexTypes.Content);
            var delCount = r.DeleteDocuments(contentTerm);                        
            r.Commit();
            r.Close();

            //make sure the content is gone. This is done with lucene APIs, not examine!
            var collector = new AllHitsCollector(false, true);
            var query = new TermQuery(contentTerm);
            s = searcher.GetSearcher(); //make sure the searcher is up do date.
            s.Search(query, collector);
            Assert.AreEqual(0, collector.Count);

            //call our indexing methods
            var indexer = GetIndexer();
            Trace.WriteLine("Indexer folder is " + indexer.LuceneIndexFolder.FullName);
            indexer.IndexAll(IndexTypes.Content);
           
            collector = new AllHitsCollector(false, true);
            s = searcher.GetSearcher(); //make sure the searcher is up do date.
            s.Search(query, collector);
            Assert.AreEqual(10, collector.Count);
        }

        ///// <summary>
        /// This will delete an item from the index and ensure that all children of the node are deleted too!
        /// </summary>
        [TestMethod]
        public void Index_Delete_Index_Item_Ensure_Heirarchy_Removed()
        {         

            //now delete a node that has children
            var indexer = GetIndexer();

            //first, rebuild index to ensure all data is there
            indexer.RebuildIndex();

            var searcher = GetSearcherProvider();

            indexer.DeleteFromIndex(1140.ToString());
            //this node had children: 1141 & 1142, let's ensure they are also removed

            var results = searcher.Search(searcher.CreateSearchCriteria().Id(1141).Compile());
            Assert.AreEqual<int>(0, results.Count());

            results = searcher.Search(searcher.CreateSearchCriteria().Id(1142).Compile());
            Assert.AreEqual<int>(0, results.Count());

        }

        #region Private methods and properties

        private TestContentService m_DataService = new TestContentService();

        /// <summary>
        /// Helper method to return the index searcher for this index
        /// </summary>
        /// <returns></returns>
        private UmbracoExamineSearcher GetSearcherProvider()
        {
            return (UmbracoExamineSearcher)ExamineManager.Instance.SearchProviderCollection["CWSSearcher"];
        }

        /// <summary>
        /// Helper method to return the indexer that we are testing
        /// </summary>
        /// <returns></returns>
        private UmbracoContentIndexer GetIndexer()
        {
            return (UmbracoContentIndexer)ExamineManager.Instance.IndexProviderCollection["CWSIndexer"];
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
