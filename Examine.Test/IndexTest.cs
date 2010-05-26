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
            var s = GetSearcherProvider();
            var r = s.GetSearcher().GetIndexReader();

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
            var searcher = GetSearcherProvider();
            Trace.WriteLine("Searcher folder is " + searcher.LuceneIndexFolder.FullName);
            var s = searcher.GetSearcher();

            //first delete all 'Content' (not media). This is done by directly manipulating the index with the Lucene API, not examine!
            var r = IndexReader.Open(s.GetIndexReader().Directory(), false);            
            var contentTerm = new Term(LuceneExamineIndexer.IndexTypeFieldName, IndexType.Content.ToString().ToLower());
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
            var indexer =  GetIndexer();
            Trace.WriteLine("Indexer folder is " + indexer.LuceneIndexFolder.FullName);
            indexer.IndexAll(IndexType.Content);
           
            collector = new AllHitsCollector(false, true);
            s = searcher.GetSearcher(); //make sure the searcher is up do date.
            s.Search(query, collector);
            Assert.AreEqual(10, collector.Count);
        }

        #region Private methods
        /// <summary>
        /// Helper method to return the index searcher for this index
        /// </summary>
        /// <returns></returns>
        private LuceneExamineSearcher GetSearcherProvider()
        {
            return (LuceneExamineSearcher)ExamineManager.Instance.SearchProviderCollection["CWSIndex"];            
        }

        /// <summary>
        /// Helper method to return the indexer that we are testing
        /// </summary>
        /// <returns></returns>
        private LuceneExamineIndexer GetIndexer()
        {
            return (LuceneExamineIndexer)ExamineManager.Instance.IndexProviderCollection["CWSIndex"];
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
