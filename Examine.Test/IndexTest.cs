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

namespace Examine.Test
{
    [TestClass]
    public class IndexTest
    {

        [TestMethod]
        public void TestRebuildIndex()
        {
            GetIndexer().RebuildIndex();

            
            //do validation...

            //get searcher and reader to get stats
            var s = GetSearcher();
            var r = s.GetIndexReader();

            //there's 15 fields in the index, but 3 sorted fields
            var fields = r.GetFieldNames(IndexReader.FieldOption.ALL);

            Assert.AreEqual(18, fields.Count());
            //ensure there's 3 sorting fields
            Assert.AreEqual(3, fields.Where(x => x.StartsWith(LuceneExamineIndexer.SortedFieldNamePrefix)).Count());
            //there should be 10 documents
            Assert.AreEqual(10, r.NumDocs());
        }

        /// <summary>
        /// This will ensure that all 'Content' (not media) is cleared from the index using the Lucene API directly.
        /// We then call the Examine method to re-index Content and do some comparisons to ensure that it worked correctly.
        /// </summary>
        [TestMethod]
        public void TestReindexContent()
        {
            var s = GetSearcher();

            //first delete all 'Content' (not media). This is done by directly manipulating the index with the Lucene API
            var r = s.GetIndexReader();
            r = r.Reopen(false); //open write
            var contentTerm = new Term(LuceneExamineIndexer.IndexTypeFieldName, IndexType.Content.ToString().ToLower());
            var delCount = r.DeleteDocuments(contentTerm);                        
            r.Commit();

            //make sure the content is gone
            s = GetSearcher();
            var collector = new AllHitsCollector(false, true);
            var query = new TermQuery(contentTerm);            
            s.Search(query, collector);
            Assert.AreEqual(0, collector.Count);

            //call our indexing methods
            GetIndexer().IndexAll(IndexType.Content);

            s = GetSearcher(); //re-get the searcher
            collector = new AllHitsCollector(false, true);
            s.Search(query, collector);
            Assert.AreEqual(10, collector.Count);
        }

        #region Private methods
        /// <summary>
        /// Helper method to return the index searcher for this index
        /// </summary>
        /// <returns></returns>
        private IndexSearcher GetSearcher()
        {
            var searcher = (LuceneExamineSearcher)ExamineManager.Instance.SearchProviderCollection["CWSIndex"];
            return searcher.GetSearcher();
        }

        /// <summary>
        /// Helper method to return the indexer that we are testing
        /// </summary>
        /// <returns></returns>
        private IIndexer GetIndexer()
        {
            var indexer = ExamineManager.Instance.IndexProviderCollection["CWSIndex"];
            return indexer;
        } 
        #endregion
        
        #region Initialize and Cleanup

        private static IndexInitializer m_Init;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
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
