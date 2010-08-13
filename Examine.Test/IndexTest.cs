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

        [TestMethod]
        public void Index_Rebuild_Index()
        {
            //get searcher and reader to get stats
            var s = (UmbracoExamineSearcher)ExamineManager.Instance.SearchProviderCollection["CWSSearcher"];
            var r = s.GetSearcher().GetIndexReader();

            Trace.Write("Num docs = " + r.NumDocs().ToString());


            var indexer = (UmbracoExamineIndexer)ExamineManager.Instance.IndexProviderCollection["CWSIndexer"]; 
            indexer.RebuildIndex();
            
            //do validation...

            //get searcher and reader to get stats
            s = (UmbracoExamineSearcher)ExamineManager.Instance.SearchProviderCollection["CWSSearcher"];
            r = s.GetSearcher().GetIndexReader();

            //there's 16 fields in the index, but 3 sorted fields
            var fields = r.GetFieldNames(IndexReader.FieldOption.ALL);

            Assert.AreEqual(19, fields.Count());
            //ensure there's 3 sorting fields
            Assert.AreEqual(3, fields.Where(x => x.StartsWith(UmbracoExamineIndexer.SortedFieldNamePrefix)).Count());
            //there should be 11 documents (10 content, 1 media)
            Assert.AreEqual(11, r.NumDocs());

            //test for the special fields to ensure they are there:
            Assert.AreEqual<int>(1, fields.Where(x => x == LuceneIndexer.IndexNodeIdFieldName).Count());
            Assert.AreEqual<int>(1, fields.Where(x => x == LuceneIndexer.IndexTypeFieldName).Count());
            Assert.AreEqual<int>(1, fields.Where(x => x == UmbracoExamineIndexer.IndexPathFieldName).Count());

        }

        /// <summary>
        /// This will ensure that all 'Content' (not media) is cleared from the index using the Lucene API directly.
        /// We then call the Examine method to re-index Content and do some comparisons to ensure that it worked correctly.
        /// </summary>
        [TestMethod]
        public void Index_Reindex_Content()
        {
            var searcher = (UmbracoExamineSearcher)ExamineManager.Instance.SearchProviderCollection["CWSSearcher"];

            Trace.WriteLine("Searcher folder is " + searcher.LuceneIndexFolder.FullName);
            var s = searcher.GetSearcher();

            //first delete all 'Content' (not media). This is done by directly manipulating the index with the Lucene API, not examine!
            var r = IndexReader.Open(s.GetIndexReader().Directory(), false);            
            var contentTerm = new Term(UmbracoExamineIndexer.IndexTypeFieldName, IndexTypes.Content.ToLower());
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
            var indexer = (UmbracoExamineIndexer)ExamineManager.Instance.IndexProviderCollection["CWSIndexer"];
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
            var indexer = (UmbracoExamineIndexer)ExamineManager.Instance.IndexProviderCollection["CWSIndexer"];

            //first, rebuild index to ensure all data is there
            indexer.RebuildIndex();

            var searcher = (UmbracoExamineSearcher)ExamineManager.Instance.SearchProviderCollection["CWSSearcher"];

            indexer.DeleteFromIndex(1140.ToString());
            //this node had children: 1141 & 1142, let's ensure they are also removed

            var results = searcher.Search(searcher.CreateSearchCriteria().Id(1141).Compile());
            Assert.AreEqual<int>(0, results.Count());

            results = searcher.Search(searcher.CreateSearchCriteria().Id(1142).Compile());
            Assert.AreEqual<int>(0, results.Count());

        }

        #region Private methods

       
       
        
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
