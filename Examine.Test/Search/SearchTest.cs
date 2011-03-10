using System;
using System.IO;
using System.Linq;
using Examine.Test.DataServices;
using Lucene.Net.Analysis.Standard;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UmbracoExamine;

namespace Examine.Test.Search
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class SearchTest
    {
       
        [TestMethod]
        public void Search_SimpleSearch()
        {
            var result = _searcher.Search("sam", false);
            Assert.AreEqual(4, result.Count(), "Results returned for 'sam' should be equal to 5 with the StandardAnalyzer");            
        }

        [TestMethod]
        public void Search_SimpleSearchWithWildcard()
        {
            var result = _searcher.Search("umb", true);
            Assert.AreEqual(7, result.Count(), "Total results for 'umb' is 8 using wildcards");
        }

        
        private static ISearcher _searcher;
        private static IIndexer _indexer;

        #region Initialize and Cleanup

        [ClassInitialize()]       
        public static void Initialize(TestContext context)
        {
            var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
            _indexer = IndexInitializer.GetUmbracoIndexer(newIndexFolder);
            _indexer.RebuildIndex();
            _searcher = IndexInitializer.GetUmbracoSearcher(newIndexFolder);
        }

        #endregion
    }
}
