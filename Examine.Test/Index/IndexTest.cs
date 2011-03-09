using System;
using System.IO;
using System.Linq;
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

namespace Examine.Test.Index
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
            //change parent id to 1116
            ((IndexCriteria)_indexer.IndexerData).ParentNodeId = 1116;

            //rebuild so it excludes children unless they are under 1116
            _indexer.RebuildIndex();

            //ensure that node 2112 doesn't exist
            var results = _searcher.Search(_searcher.CreateSearchCriteria().Id(2112).Compile());
            Assert.AreEqual<int>(0, results.Count());

            //get a node from the data repo (this one exists underneath 2222)
            var node = _mediaService.GetLatestMediaByXpath("//*[string-length(@id)>0 and number(@id)>0]")
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
            _indexer.ReIndexNode(node, IndexTypes.Media);

            //RESET the parent id
            ((IndexCriteria)_indexer.IndexerData).ParentNodeId = null;

            //now ensure it's deleted
            var newResults = _searcher.Search(_searcher.CreateSearchCriteria().Id(2112).Compile());
            Assert.AreEqual(1, newResults.Count());

        }

        [TestMethod]
        public void Index_Move_Media_To_Non_Indexable_ParentID()
        {
            //get a node from the data repo (this one exists underneath 2222)
            var node = _mediaService.GetLatestMediaByXpath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .Where(x => (int)x.Attribute("id") == 2112)
                .First();

            var currPath = (string)node.Attribute("path"); //should be : -1,2222,2112
            Assert.AreEqual("-1,2222,2112", currPath);
            
            //ensure it's indexed
            _indexer.ReIndexNode(node, IndexTypes.Media);

            //change the parent node id to be the one it used to exist under
            ((IndexCriteria)_indexer.IndexerData).ParentNodeId = 2222;

            //now mimic moving the node underneath 1116 instead of 2222
            node.SetAttributeValue("path", currPath.Replace("2222", "1116"));
            node.SetAttributeValue("parentID", "1116");

            //now reindex the node, this should first delete it and then NOT add it because of the parent id constraint
            _indexer.ReIndexNode(node, IndexTypes.Media);

            //RESET the parent id
            ((IndexCriteria)_indexer.IndexerData).ParentNodeId = null;

            //now ensure it's deleted
            var results = _searcher.Search(_searcher.CreateSearchCriteria().Id(2112).Compile());
            Assert.AreEqual(0, results.Count());

        }

        /// <summary>
        /// This will create a new index queue item for the same ID multiple times to ensure that the 
        /// index does not end up with duplicate entries.
        /// </summary>
        [TestMethod]
        public void Index_Ensure_No_Duplicates_In_Async()
        {           
            //add the handler
            var handler = new EventHandler<IndexedNodesEventArgs>(indexer_NodesIndexed);
            _indexer.NodesIndexed += handler;

            //run in async mode
            _indexer.RunAsync = true;

            //get a node from the data repo
            var node = _contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .First();

            //get the id for th node we're re-indexing.
            var id = (int)node.Attribute("id");

            //set our internal monitoring flag
            _isIndexing = true;

            //reindex the same node 210 times
            for (var i = 0; i < 210; i++)
            {
                _indexer.ReIndexNode(node, IndexTypes.Content);
            }

            //we need to check if the indexing is complete
            while (_isIndexing)
            {
                //wait until indexing is done
                Thread.Sleep(1000);
            }

            //reset the async mode and remove event handler
            _indexer.RunAsync = false;
            _indexer.NodesIndexed -= handler;

            //ensure no duplicates
            var results = _searcher.Search(_searcher.CreateSearchCriteria().Id(id).Compile());
            Assert.AreEqual(1, results.Count());
        }

        [TestMethod]
        public void Index_Ensure_No_Duplicates_In_Non_Async()
        {
            //get a node from the data repo
            var node = _contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .First();

            //get the id for th node we're re-indexing.
            var id = (int)node.Attribute("id");

            //reindex the same node 210 times
            for (var i = 0; i < 210; i++)
            {
                _indexer.ReIndexNode(node, IndexTypes.Content);
            }
           
            //ensure no duplicates
            var results = _searcher.Search(_searcher.CreateSearchCriteria().Id(id).Compile());
            Assert.AreEqual(1, results.Count());
        }

        /// <summary>
        /// Used to monitor async operation
        /// </summary>
        private bool _isIndexing = false;
        
        /// <summary>
        /// Used to monitor an Async operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void indexer_NodesIndexed(object sender, IndexedNodesEventArgs e)
        {
            _isIndexing = false;
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

            EventHandler<DeleteIndexEventArgs> indexDeletedHandler = (sender, e) =>
            {
                isDeleted = true;
                Assert.IsFalse(isAdded, "node was added before it was deleted!");
            };

            //add index deleted event handler
            _indexer.IndexDeleted += indexDeletedHandler;

            EventHandler<IndexedNodeEventArgs> nodeIndexedHandler = (sender, e) =>
            {
                isAdded = true;
                Assert.IsTrue(isDeleted, "node was not deleted first!");
            };

            //add index added event handler
            _indexer.NodeIndexed += nodeIndexedHandler;
            //get a node from the data repo
            var node = _contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .First();

            //this will do the reindex (deleting, then updating)
            _indexer.ReIndexNode(node, IndexTypes.Content);

            _indexer.IndexDeleted -= indexDeletedHandler;
            _indexer.NodeIndexed -= nodeIndexedHandler;

            Assert.IsTrue(isDeleted, "node was not deleted");
            Assert.IsTrue(isAdded, "node was not re-added");
        }


         


        [TestMethod]
        public void Index_Rebuild_Index()
        {
            //get searcher and reader to get stats
            var r = ((IndexSearcher)_searcher.GetSearcher()).GetIndexReader();   
                                    
            //there's 16 fields in the index, but 3 sorted fields
            var fields = r.GetFieldNames(IndexReader.FieldOption.ALL);

            Assert.AreEqual(21, fields.Count());
            //ensure there's 3 sorting fields
            Assert.AreEqual(3, fields.Where(x => x.StartsWith(LuceneIndexer.SortedFieldNamePrefix)).Count());
            //there should be 11 documents (10 content, 1 media)
            Assert.AreEqual(11, r.NumDocs());

            //test for the special fields to ensure they are there:
            Assert.AreEqual(1, fields.Where(x => x == LuceneIndexer.IndexNodeIdFieldName).Count());
            Assert.AreEqual(1, fields.Where(x => x == LuceneIndexer.IndexTypeFieldName).Count());
            Assert.AreEqual(1, fields.Where(x => x == UmbracoContentIndexer.IndexPathFieldName).Count());
            Assert.AreEqual(1, fields.Where(x => x == UmbracoContentIndexer.NodeTypeAliasFieldName).Count());

        }

        /// <summary>
        /// This will ensure that all 'Content' (not media) is cleared from the index using the Lucene API directly.
        /// We then call the Examine method to re-index Content and do some comparisons to ensure that it worked correctly.
        /// </summary>
        [TestMethod]
        public void Index_Reindex_Content()
        {
            Trace.WriteLine("Searcher folder is " + _searcher.LuceneIndexFolder.FullName);
            var s = (IndexSearcher)_searcher.GetSearcher();

            //first delete all 'Content' (not media). This is done by directly manipulating the index with the Lucene API, not examine!
            var r = IndexReader.Open(s.GetIndexReader().Directory(), false);            
            var contentTerm = new Term(UmbracoContentIndexer.IndexTypeFieldName, IndexTypes.Content);
            var delCount = r.DeleteDocuments(contentTerm);                        
            r.Commit();
            r.Close();

            //make sure the content is gone. This is done with lucene APIs, not examine!
            var collector = new AllHitsCollector(false, true);
            var query = new TermQuery(contentTerm);
            s = (IndexSearcher)_searcher.GetSearcher(); //make sure the searcher is up do date.
            s.Search(query, collector);
            Assert.AreEqual(0, collector.Count);

            //call our indexing methods
            Trace.WriteLine("Indexer folder is " + _indexer.LuceneIndexFolder.FullName);
            _indexer.IndexAll(IndexTypes.Content);
           
            collector = new AllHitsCollector(false, true);
            s = (IndexSearcher)_searcher.GetSearcher(); //make sure the searcher is up do date.
            s.Search(query, collector);
            Assert.AreEqual(10, collector.Count);
        }

        /// <summary>
        /// This will delete an item from the index and ensure that all children of the node are deleted too!
        /// </summary>
        [TestMethod]
        public void Index_Delete_Index_Item_Ensure_Heirarchy_Removed()
        {         

            //now delete a node that has children

            _indexer.DeleteFromIndex(1140.ToString());
            //this node had children: 1141 & 1142, let's ensure they are also removed

            var results = _searcher.Search(_searcher.CreateSearchCriteria().Id(1141).Compile());
            Assert.AreEqual(0, results.Count());

            results = _searcher.Search(_searcher.CreateSearchCriteria().Id(1142).Compile());
            Assert.AreEqual(0, results.Count());

        }

        #region Private methods and properties

        private readonly TestContentService _contentService = new TestContentService();
        private readonly TestMediaService _mediaService = new TestMediaService();

        private UmbracoExamineSearcher _searcher;
        private UmbracoContentIndexer _indexer;

        #endregion

        #region Initialize and Cleanup
        

        /// <summary>
        /// Run before every test!
        /// </summary>
        [TestInitialize()]
        public void Initialize()
        {
            var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
            _indexer = IndexInitializer.GetCwsIndexer(newIndexFolder);
            _indexer.RebuildIndex();
            _searcher = IndexInitializer.GetUmbracoSearcher(newIndexFolder);
        }


        #endregion
    }
}
