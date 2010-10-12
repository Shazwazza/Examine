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



        [TestMethod]
        public void Index_Move_Media_From_Non_Indexable_To_Indexable_ParentID()
        {
            var indexer = GetIndexer();

            //change parent id to 1116
            ((IndexCriteria)indexer.IndexerData).ParentNodeId = 1116;

            //rebuild so it excludes children unless they are under 1116
            indexer.RebuildIndex();

            //ensure that node 2112 doesn't exist
            var search = GetSearcherProvider();
            var results = search.Search(search.CreateSearchCriteria().Id(2112).Compile());
            Assert.AreEqual<int>(0, results.Count());

            //get a node from the data repo (this one exists underneath 2222)
            var node = m_MediaService.GetLatestMediaByXpath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .Where(x => (int)x.Attribute("id") == 2112)
                .First();

            var currPath = (string)node.Attribute("path"); //should be : -1,2222,2112
            Assert.AreEqual("-1,2222,2112", currPath);

            //now mimic moving 2112 to 1116
            node.SetAttributeValue("path", currPath.Replace("2222", "1116"));
            node.SetAttributeValue("parentID", "1116");

            //now reindex the node, this should first delete it and then WILL add it because of the parent id constraint
            indexer.ReIndexNode(node, IndexTypes.Media);

            //RESET the parent id
            ((IndexCriteria)indexer.IndexerData).ParentNodeId = null;

            //now ensure it's deleted
            var newResults = search.Search(search.CreateSearchCriteria().Id(2112).Compile());
            Assert.AreEqual<int>(1, newResults.Count());

        }

        [TestMethod]
        public void Index_Move_Media_To_Non_Indexable_ParentID()
        {
            var indexer = GetIndexer();

            //get a node from the data repo (this one exists underneath 2222)
            var node = m_MediaService.GetLatestMediaByXpath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .Where(x => (int)x.Attribute("id") == 2112)
                .First();

            var currPath = (string)node.Attribute("path"); //should be : -1,2222,2112
            Assert.AreEqual("-1,2222,2112", currPath);
            
            //ensure it's indexed
            indexer.ReIndexNode(node, IndexTypes.Media);

            //change the parent node id to be the one it used to exist under
            ((IndexCriteria)indexer.IndexerData).ParentNodeId = 2222;

            //now mimic moving the node underneath 1116 instead of 2222
            node.SetAttributeValue("path", currPath.Replace("2222", "1116"));
            node.SetAttributeValue("parentID", "1116");

            //now reindex the node, this should first delete it and then NOT add it because of the parent id constraint
            indexer.ReIndexNode(node, IndexTypes.Media);

            //RESET the parent id
            ((IndexCriteria)indexer.IndexerData).ParentNodeId = null;

            //now ensure it's deleted
            var search = GetSearcherProvider();
            var results = search.Search(search.CreateSearchCriteria().Id(2112).Compile());
            Assert.AreEqual<int>(0, results.Count());

        }

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
            var node = m_ContentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
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
            var node = m_ContentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
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

            EventHandler<DeleteIndexEventArgs> indexDeletedHandler = (sender, e) =>
            {
                isDeleted = true;
                Assert.IsFalse(isAdded, "node was added before it was deleted!");
            };

            //add index deleted event handler
            indexer.IndexDeleted += indexDeletedHandler;

            EventHandler<IndexedNodeEventArgs> nodeIndexedHandler = (sender, e) =>
            {
                isAdded = true;
                Assert.IsTrue(isDeleted, "node was not deleted first!");
            };

            //add index added event handler
            indexer.NodeIndexed += nodeIndexedHandler;
            //get a node from the data repo
            var node = m_ContentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .First();

            //this will do the reindex (deleting, then updating)
            indexer.ReIndexNode(node, IndexTypes.Content);

            indexer.IndexDeleted -= indexDeletedHandler;
            indexer.NodeIndexed -= nodeIndexedHandler;

            Assert.IsTrue(isDeleted, "node was not deleted");
            Assert.IsTrue(isAdded, "node was not re-added");
        }


         


        [TestMethod]
        public void Index_Rebuild_Index()
        {
            //get searcher and reader to get stats
            var s = GetSearcherProvider();
            var r = s.GetSearcher().GetIndexReader();            
            var indexer = GetIndexer();
                        
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

            var searcher = GetSearcherProvider();

            indexer.DeleteFromIndex(1140.ToString());
            //this node had children: 1141 & 1142, let's ensure they are also removed

            var results = searcher.Search(searcher.CreateSearchCriteria().Id(1141).Compile());
            Assert.AreEqual<int>(0, results.Count());

            results = searcher.Search(searcher.CreateSearchCriteria().Id(1142).Compile());
            Assert.AreEqual<int>(0, results.Count());

        }

        #region Private methods and properties

        private TestContentService m_ContentService = new TestContentService();
        private TestMediaService m_MediaService = new TestMediaService();

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

        /// <summary>
        /// Run before every test!
        /// </summary>
        [TestInitialize()]
        public void Initialize()
        {
            m_Init = new IndexInitializer();

            GetIndexer().RebuildIndex();
        }


        #endregion
    }
}
