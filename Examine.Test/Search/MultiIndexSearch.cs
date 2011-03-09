using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Examine.Test.Search
{
    [TestClass]
    public class MultiIndexSearch
    {
        [TestMethod]
        public void MultiIndex_Simple_Search()
        {
            var result = _searcher.Search("sam", false);            
            Assert.AreEqual(11, result.Count(), "Results returned for 'sam' should be equal to 5 with the StandardAnalyzer");            
        }

        [TestMethod]
        public void MultiIndex_Field_Count()
        {
            var result = _searcher.GetSearchFields();
            Assert.AreEqual(54, result.Count(), "The total number for fields between all of the indexes should be ");
        }

        #region Initialize and Cleanup

        private MultiIndexSearcher _searcher;

        [TestInitialize]
        public void Initialize()
        {


            //IndexInitializer.Initialize();
            //_searcher = ExamineManager.Instance.SearchProviderCollection["MultiIndexSearcher"];

            ////ensure we're re-indexed before testing
            //foreach(var i in ExamineManager.Instance.IndexProviderCollection.Cast<IIndexer>())
            //{
            //    i.RebuildIndex();
            //}
        }
        
        #endregion
    }
}
