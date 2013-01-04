using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine;
using Lucene.Net.Search;
using NUnit.Framework;

namespace Examine.Test.Search
{
    [TestFixture]
    public class HighlightResultsTest
    {
        [Test]
        public void HighlightResults_Do_Highlight()
        {
            //TODO: Get highlighter lib

            //var result = (SearchResults)_searcher.Search("sam", false);
           
            //var searcher = (IndexSearcher)result.LuceneSearcher;
            //var query = result.LuceneQuery;

            //var scorer = new QueryScorer(query);
            //var tokenStream = HighlightAnalyzer.TokenStream(highlightField, new StringReader(value));
            //return highlighter.GetBestFragments(tokenStream, value, MaxNumHighlights, Separator);

            //Assert.AreEqual(4, result.Count(), "Results returned for 'sam' should be equal to 5 with the StandardAnalyzer");
            Assert.Inconclusive();

        }


        private static ISearcher _searcher;
        private static IIndexer _indexer;

        [SetUp]
        public void Initialize()
        {
            var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
            _indexer = IndexInitializer.GetUmbracoIndexer(newIndexFolder);
            _indexer.RebuildIndex();
            _searcher = IndexInitializer.GetUmbracoSearcher(newIndexFolder);
        }

		[TearDown]
		public void TearDown()
		{
			var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
			TestHelper.CleanupFolder(newIndexFolder.Parent);
		}
    }
}
