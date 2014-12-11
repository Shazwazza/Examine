using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine;
using Examine.Test.PartialTrust;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;

namespace Examine.Test.Search
{
    //[TestFixture]
    //public class HighlightResultsTest : AbstractPartialTrustFixture<HighlightResultsTest>
    //{
    //    [Test]
    //    public void HighlightResults_Do_Highlight()
    //    {
    //        //TODO: Get highlighter lib

    //        //var result = (SearchResults)_searcher.Search("sam", false);
           
    //        //var searcher = (IndexSearcher)result.LuceneSearcher;
    //        //var query = result.LuceneQuery;

    //        //var scorer = new QueryScorer(query);
    //        //var tokenStream = HighlightAnalyzer.TokenStream(highlightField, new StringReader(value));
    //        //return highlighter.GetBestFragments(tokenStream, value, MaxNumHighlights, Separator);

    //        //Assert.AreEqual(4, result.Count(), "Results returned for 'sam' should be equal to 5 with the StandardAnalyzer");
    //        Assert.Inconclusive();

    //    }


    //    private static ISearcher _searcher;
    //    private static IIndexer _indexer;
    //    private Lucene.Net.Store.Directory _luceneDir;

    //    public override void TestSetup()
    //    {
    //        _luceneDir = new RAMDirectory();
    //        _indexer = IndexInitializer.GetUmbracoIndexer(_luceneDir);
    //        _indexer.RebuildIndex();
    //        _searcher = IndexInitializer.GetUmbracoSearcher(_luceneDir);
    //    }

    //    public override void TestTearDown()
    //    {
    //        _luceneDir.Dispose();
    //    }
    //}
}
