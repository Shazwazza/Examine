using System;
using System.IO;
using System.Linq;
using Examine.Test.DataServices;
using Examine.Test.PartialTrust;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using NUnit.Framework;
using UmbracoExamine;

namespace Examine.Test.Search
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestFixture]
	public class SearchTest : AbstractPartialTrustFixture<SearchTest>
    {


        [Test]
        public void Search_On_Stop_Word()
        {
           
            var result = _searcher.Search("into", false);
            var result2 = _searcher.Search("into sam", false);

            Assert.AreEqual(0, result.TotalItemCount);
            Assert.AreEqual(0, result.Count());

            Assert.IsTrue(result2.TotalItemCount > 0);
            Assert.IsTrue(result2.Count() > 0);
        }

        [Test]
        public void Search_SimpleSearch()
        {
            var result = _searcher.Search("sam", false);
            Assert.AreEqual(4, result.Count(), "Results returned for 'sam' should be equal to 5 with the StandardAnalyzer");            
        }

        [Test]
        public void Search_SimpleSearchWithWildcard()
        {
            var result = _searcher.Search("umb", true);
            Assert.AreEqual(7, result.Count(), "Total results for 'umb' is 8 using wildcards");
        }

        
        private static ISearcher _searcher;
        private static IIndexer _indexer;
		private Lucene.Net.Store.Directory _luceneDir;

        #region Initialize and Cleanup

		public override void TestSetup()
        {
			_luceneDir = new RAMDirectory();
			_indexer = IndexInitializer.GetUmbracoIndexer(_luceneDir);
            _indexer.RebuildIndex();
			_searcher = IndexInitializer.GetUmbracoSearcher(_luceneDir);
        }

		public override void TestTearDown()
		{
			_luceneDir.Dispose();	
		}

        #endregion
    }
}
