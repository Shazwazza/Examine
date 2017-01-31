using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Providers;
using System.IO;
using Examine.LuceneEngine;
using Lucene.Net.Store;
using NUnit.Framework;

namespace Examine.Test.Search
{
	
    [TestFixture]
    public class MultiIndexSearch
    {
        [Test]
        public void MultiIndex_Simple_Search()
        {
            using (var cwsDir = new RandomIdRAMDirectory())
            using (var pdfDir = new RandomIdRAMDirectory())
            using (var simpleDir = new RandomIdRAMDirectory())
            using (var conventionDir = new RandomIdRAMDirectory())
            {
                //get all of the indexers and rebuild them all first
                var indexers = new IIndexer[]
                                   {
                                   IndexInitializer.GetUmbracoIndexer(cwsDir),
                                   IndexInitializer.GetSimpleIndexer(simpleDir),
                                   IndexInitializer.GetUmbracoIndexer(conventionDir)
                                   };
                foreach (var i in indexers)
                {
                    i.RebuildIndex();
                }

                using (var cwsIndexer = IndexInitializer.GetUmbracoIndexer(cwsDir))
                {
                    cwsIndexer.RebuildIndex();
                    //now get the multi index searcher for all indexes
                    using (var searcher = IndexInitializer.GetMultiSearcher(pdfDir, simpleDir, conventionDir, cwsDir))                
                    using (var cwsSearcher = IndexInitializer.GetUmbracoSearcher(cwsDir))
                    {
                        var cwsResult = cwsSearcher.Search("sam", false);
                        var result = searcher.Search("sam", false);

                        //ensure there's more results than just the one index
                        Assert.IsTrue(cwsResult.Count() < result.Count());
                        //there should be 8
                        Assert.AreEqual(8, result.Count(), "Results returned for 'sam' should be equal to 5 with the StandardAnalyzer");
                    }
                };

            }

            
                      
        }

        [Test]
        public void MultiIndex_Field_Count()
        {
            using (var cwsDir = new RandomIdRAMDirectory())
            using (var pdfDir = new RandomIdRAMDirectory())
            using (var simpleDir = new RandomIdRAMDirectory())
            using (var conventionDir = new RandomIdRAMDirectory())
            {
                //get all of the indexers and rebuild them all first
                var indexers = new IIndexer[]
                                   {
                                   IndexInitializer.GetUmbracoIndexer(cwsDir),
                                   IndexInitializer.GetSimpleIndexer(simpleDir),
                                   IndexInitializer.GetUmbracoIndexer(conventionDir)
                                   };
                foreach (var i in indexers)
                {

                    i.RebuildIndex();
                }

                //now get the multi index searcher for all indexes
                using (var searcher = IndexInitializer.GetMultiSearcher(pdfDir, simpleDir, conventionDir, cwsDir))
                {
                    var result = searcher.GetSearchFields();
                    Assert.AreEqual(26, result.Count(), "The total number for fields between all of the indexes should be ");
                }
            }
        }

        
    }
}
