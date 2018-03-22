using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Examine.LuceneEngine;
using Examine.LuceneEngine.SearchCriteria;
using Examine.Test.DataServices;

using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using UmbracoExamine;

namespace Examine.Test.Search
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestFixture]
	public class SearchTest //: AbstractPartialTrustFixture<SearchTest>
    {

        [Test]
        public void Match_Search_Field_Sort_Syntax()
        {
            var val = "myFieldName[Type=INT]";
            var match = LuceneSearchCriteria.SortMatchExpression.Match(val);

            Assert.IsTrue(match.Success);
            var type = match.Groups["type"];
            Assert.IsNotNull(type);
            Assert.AreEqual("INT", type.Value);
            Assert.AreEqual("myFieldName", val.Substring(0, match.Index));
        }

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

        [TestFixtureSetUp]
        public void TestSetup()
        {
			_luceneDir = new RandomIdRAMDirectory();
			_indexer = IndexInitializer.GetUmbracoIndexer(_luceneDir);
            _indexer.RebuildIndex();
			_searcher = IndexInitializer.GetUmbracoSearcher(_luceneDir);
        }

        [TestFixtureTearDown]
        public void TestTearDown()
		{
			_luceneDir.Dispose();	
		}

        #endregion
    }
}
