using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UmbracoExamine;
using Lucene.Net.Search;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;

namespace Examine.Test
{
    [TestClass]
    public class IndexTest
    {
        #region Initialize and Cleanup

        private static IndexInit m_Init;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            m_Init = new IndexInit();
        }

        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            //IndexInit.RemoveWorkingIndex();
        }

        #endregion

        /// <summary>
        /// Helper method to return the index searcher for this index
        /// </summary>
        /// <returns></returns>
        private IndexSearcher GetSearcher()
        {
            var searcher = (LuceneExamineSearcher)ExamineManager.Instance.SearchProviderCollection["CWSIndex"];
            searcher.ValidateSearcher(true);
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

        [TestMethod]
        public void TestRebuildIndex()
        {
            GetIndexer().RebuildIndex();

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

            //ensure searcher and reader are closed.
            r.Close();
            s.Close();
        }

        [TestMethod]
        public void TestReindexContent()
        {
            //first delete everything
            var s = GetSearcher();
            
            var r = s.GetIndexReader();
            r = r.Reopen(false); //open write
            var contentTerm = new Term(LuceneExamineIndexer.IndexTypeFieldName, IndexType.Content.ToString().ToLower());
            var delCount = r.DeleteDocuments(contentTerm);
            r.Close();
            
            //make sure the content is gone
            s = GetSearcher(); //re-get the searcher 
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

            s.Close();
        }
    }
}
