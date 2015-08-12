using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Providers;
using System.IO;
using Lucene.Net.Store;
using NUnit.Framework;

namespace Examine.Test.Search
{
	//TODO: Convert to use partial trust helpers but itextsharp is the culprit here

    [TestFixture]
    public class MultiIndexSearch
    {
        [Test]
        public void MultiIndex_Simple_Search()
        {
            using (var cwsIndexer = IndexInitializer.GetUmbracoIndexer(_cwsDir))
            {
                cwsIndexer.RebuildIndex();
                using (var cwsSearcher = IndexInitializer.GetUmbracoSearcher(_cwsDir))
                {
                    var cwsResult = cwsSearcher.Search("sam", false);
                    var result = _searcher.Search("sam", false);

                    //ensure there's more results than just the one index
                    Assert.IsTrue(cwsResult.Count() < result.Count());
                    //there should be 8
                    Assert.AreEqual(8, result.Count(), "Results returned for 'sam' should be equal to 5 with the StandardAnalyzer");  
                }
            };
                      
        }

        [Test]
        public void MultiIndex_Field_Count()
        {
            var result = _searcher.GetSearchFields();
            Assert.AreEqual(26, result.Count(), "The total number for fields between all of the indexes should be ");
        }

        #region Initialize and Cleanup

        private static MultiIndexSearcher _searcher;
	    private Lucene.Net.Store.Directory _cwsDir;
		private Lucene.Net.Store.Directory _pdfDir;
		private Lucene.Net.Store.Directory _simpleDir;
	    private Lucene.Net.Store.Directory _conventionDir;

        [SetUp]
        public void Initialize()
        {
			_cwsDir = new RAMDirectory();
			_pdfDir = new RAMDirectory();
			_simpleDir = new RAMDirectory();
			_conventionDir = new RAMDirectory();
            
            //get all of the indexers and rebuild them all first
            var indexers = new IIndexer[]
                               {
                                   IndexInitializer.GetUmbracoIndexer(_cwsDir),                                   
                                   IndexInitializer.GetSimpleIndexer(_simpleDir),
                                   IndexInitializer.GetUmbracoIndexer(_conventionDir)
                               };            
            foreach (var i in indexers)
            {
                try
                {
                    i.RebuildIndex();
                }
                finally
                {
                    var d = i as IDisposable;
                    if (d != null) d.Dispose();
                }
            }

            //now get the multi index searcher for all indexes
            _searcher = IndexInitializer.GetMultiSearcher(_pdfDir, _simpleDir, _conventionDir, _cwsDir);
        }

		[TearDown]
		public void TearDown()
		{
			_cwsDir.Dispose();
			_pdfDir.Dispose();
			_simpleDir.Dispose();
			_conventionDir.Dispose();
            _searcher.Dispose();
		}

        #endregion
    }
}
