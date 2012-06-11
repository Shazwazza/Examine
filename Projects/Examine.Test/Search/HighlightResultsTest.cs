using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lucene.Net.Search;

namespace Examine.Test.Search
{
    [TestClass]
    public class HighlightResultsTest
    {
        [TestMethod]
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

        [ClassInitialize()]
        public static void Initialize(TestContext context)
        {
            var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\CWSIndexSetTest", Guid.NewGuid().ToString()));
            _indexer = IndexInitializer.GetUmbracoIndexer(newIndexFolder);
            _indexer.RebuildIndex();
            _searcher = IndexInitializer.GetUmbracoSearcher(newIndexFolder);
        }
    }
}
