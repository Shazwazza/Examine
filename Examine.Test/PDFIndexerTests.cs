using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UmbracoExamine;
using System.Diagnostics;

namespace Examine.Test
{
    [TestClass]
    public class PDFIndexerTests
    {
        #region Private methods

        /// <summary>
        /// Helper method to return the index searcher for this index
        /// </summary>
        /// <returns></returns>
        private UmbracoExamineSearcher GetSearcherProvider()
        {
            return (UmbracoExamineSearcher)ExamineManager.Instance.SearchProviderCollection["PDFSearcher"];
        }

        /// <summary>
        /// Helper method to return the indexer that we are testing
        /// </summary>
        /// <returns></returns>
        private PDFIndexer GetIndexer()
        {
            return (PDFIndexer)ExamineManager.Instance.IndexProviderCollection["PDFIndexer"];
        }
        #endregion

        private static IndexInitializer m_Init;

        [TestInitialize()]
        public void Initialize()
        {
            m_Init = new IndexInitializer();
        }

        [TestMethod]
        public void PDFIndexer_Reindex()
        {
            var indexer = GetIndexer();

            indexer.RebuildIndex();

            //get searcher and reader to get stats
            var s = GetSearcherProvider();
            var r = s.GetSearcher().GetIndexReader();

            Trace.Write("Num docs = " + r.NumDocs().ToString());

            //there should be 3
            Assert.AreEqual<int>(3, r.NumDocs());

            //search the pdf content to ensure it's there
            Assert.IsTrue(s.Search(s.CreateSearchCriteria().Id(1113).Compile()).Single().Fields[PDFIndexer.TextContentFieldName].Trim().Length > 0);
            Assert.IsTrue(s.Search(s.CreateSearchCriteria().Id(1114).Compile()).Single().Fields[PDFIndexer.TextContentFieldName].Trim().Length > 0);

        }
    }
}
