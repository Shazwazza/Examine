using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Examine.Test.Search
{
    [TestClass]
    public class MultiIndexSearch
    {
        [TestMethod]
        public void MultiIndex_Simple_Search()
        {
            var di = new DirectoryInfo(Path.Combine("App_Data\\TempIndex", Guid.NewGuid().ToString()));
            var cwsIndexer = IndexInitializer.GetUmbracoIndexer(di);
            cwsIndexer.RebuildIndex();
            var cwsSearcher = IndexInitializer.GetUmbracoSearcher(di);
            
            var cwsResult = cwsSearcher.Search("sam", false);
            var result = _searcher.Search("sam", false);

            //ensure there's more results than just the one index
            Assert.IsTrue(cwsResult.Count() < result.Count());
            //there should be 10
            Assert.AreEqual(10, result.Count(), "Results returned for 'sam' should be equal to 5 with the StandardAnalyzer");            
        }

        [TestMethod]
        public void MultiIndex_Field_Count()
        {
            var result = _searcher.GetSearchFields();
            Assert.AreEqual(28, result.Count(), "The total number for fields between all of the indexes should be ");
        }

        #region Initialize and Cleanup

        private static MultiIndexSearcher _searcher;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {

            var pdfDir = new DirectoryInfo(Path.Combine("App_Data\\PDFIndexSet", Guid.NewGuid().ToString()));
            var simpleDir = new DirectoryInfo(Path.Combine("App_Data\\SimpleIndexSet", Guid.NewGuid().ToString()));
            var conventionDir = new DirectoryInfo(Path.Combine("App_Data\\ConvensionNamedTest", Guid.NewGuid().ToString()));
            var cwsDir = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));

            //get all of the indexers and rebuild them all first
            var indexers = new IIndexer[]
                               {
                                   IndexInitializer.GetUmbracoIndexer(cwsDir),
                                   IndexInitializer.GetPdfIndexer(pdfDir),
                                   IndexInitializer.GetSimpleIndexer(simpleDir),
                                   IndexInitializer.GetUmbracoIndexer(conventionDir)
                               };            
            foreach (var i in indexers)
            {
                i.RebuildIndex();
            }

            //now get the multi index searcher for all indexes
            _searcher = IndexInitializer.GetMultiSearcher(pdfDir, simpleDir, conventionDir, cwsDir);
        }
        
        #endregion
    }
}
