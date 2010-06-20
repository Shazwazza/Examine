using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UmbracoExamine;

namespace Examine.Test
{
    [TestClass]
    public class FileIndexerTests
    {
        #region Private methods

        /// <summary>
        /// Helper method to return the index searcher for this index
        /// </summary>
        /// <returns></returns>
        private LuceneExamineSearcher GetSearcherProvider()
        {
            return (LuceneExamineSearcher)ExamineManager.Instance.SearchProviderCollection["FileSearcher"];
        }

        /// <summary>
        /// Helper method to return the indexer that we are testing
        /// </summary>
        /// <returns></returns>
        private LuceneExamineIndexer GetIndexer()
        {
            return (LuceneExamineIndexer)ExamineManager.Instance.IndexProviderCollection["FileIndexer"];
        }
        #endregion

        private static IndexInitializer m_Init;

        [TestInitialize()]
        public void Initialize()
        {
            m_Init = new IndexInitializer();
        }

        [TestMethod]
        public void FileIndexerTests_Index_Pdf()
        {
            //Arrange
            var indexer = GetIndexer();

            indexer.IndexAll("Media");

            //Act

            //Assert

        }
    }
}
