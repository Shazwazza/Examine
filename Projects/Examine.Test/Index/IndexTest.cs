using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Examine.Test.PartialTrust;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using NUnit.Framework;

using Lucene.Net.Search;
using Lucene.Net.Index;
using System.Diagnostics;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using System.Threading;
using Examine.Test.DataServices;
using UmbracoExamine;

namespace Examine.Test.Index
{

    /// <summary>
    /// Tests the standard indexing capabilities
    /// </summary>
    [TestFixture]
	public class IndexTest //: AbstractPartialTrustFixture<IndexTest>
    {

        //[Test]
        //public void Add_Sortable_Field_Api()
        //{
        //    _indexer.DocumentWriting += ExternalIndexer_DocumentWriting;
        //    _indexer.RebuildIndex();
        //}

        //void ExternalIndexer_DocumentWriting(object sender, DocumentWritingEventArgs e)
        //{
        //    var field = e.Document.GetFieldable("writerName");
        //    if (field != null)
        //    {
        //        var sortedField = new Field(
        //            LuceneIndexer.SortedFieldNamePrefix + field.Name(),
        //            field.StringValue(),
        //            Field.Store.NO, //we don't want to store the field because we're only using it to sort, not return data
        //            Field.Index.NOT_ANALYZED,
        //            Field.TermVector.NO);
        //        e.Document.Add(sortedField);
        //    }
        //}

        /// <summary>
        /// This will create a new index queue item for the same ID multiple times to ensure that the 
        /// index does not end up with duplicate entries.
        /// </summary>
        [Test]
        public void Index_Ensure_No_Duplicates_In_Async()
        {

	        using (var d = new RAMDirectory())
	        {
				var customIndexer = IndexInitializer.GetUmbracoIndexer(d);

				var isIndexing = false;

				EventHandler operationComplete = (sender, e) =>
				{
					isIndexing = false;
				};

				//add the handler for optimized since we know it will be optimized last based on the commit count
				customIndexer.IndexOperationComplete += operationComplete;

				//remove the normal indexing error handler
				customIndexer.IndexingError -= IndexInitializer.IndexingError;

				//run in async mode
				customIndexer.RunAsync = true;

				//get a node from the data repo
				var node = _contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
					.Root
					.Elements()
					.First();

				//get the id for th node we're re-indexing.
				var id = (int)node.Attribute("id");

				//set our internal monitoring flag
				isIndexing = true;

				//reindex the same node a bunch of times
				for (var i = 0; i < 29; i++)
				{
					customIndexer.ReIndexNode(node, IndexTypes.Content);
				}

				//we need to check if the indexing is complete
				while (isIndexing)
				{
					//wait until indexing is done
					Thread.Sleep(1000);
				}

				//reset the async mode and remove event handler
				customIndexer.IndexOptimized -= operationComplete;
				customIndexer.IndexingError += IndexInitializer.IndexingError;
				customIndexer.RunAsync = false;


				//ensure no duplicates
				Thread.Sleep(10000); //seems to take a while to get its shit together... this i'm not sure why since the optimization should have def finished (and i've stepped through that code!)
				var customSearcher = IndexInitializer.GetLuceneSearcher(d);
				var results = customSearcher.Search(customSearcher.CreateSearchCriteria().Id(id).Compile());
				Assert.AreEqual(1, results.Count());                
	        }            
        }

        [Test]
        public void Index_Read_And_Write_Ensure_No_Errors_In_Async()
        {
            using (var d = new RAMDirectory())
            {
                using (var writer = new IndexWriter(d, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29), IndexWriter.MaxFieldLength.LIMITED))
                {
                    var customIndexer = IndexInitializer.GetUmbracoIndexer(writer);
                    var customSearcher = IndexInitializer.GetUmbracoSearcher(writer);

                    var isIndexing = false;

                    EventHandler operationComplete = (sender, e) =>
                    {
                        isIndexing = false;
                    };

                    //add the handler for optimized since we know it will be optimized last based on the commit count
                    customIndexer.IndexOperationComplete += operationComplete;

                    //remove the normal indexing error handler
                    customIndexer.IndexingError -= IndexInitializer.IndexingError;

                    //run in async mode
                    customIndexer.RunAsync = true;

                    //get a node from the data repo
                    var node = _contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                        .Root
                        .Elements()
                        .First();

                    //set our internal monitoring flag
                    isIndexing = true;

                    var docId = 1234;
                    var searchThreadCount = 500;
                    var indexThreadCount = 1;
                    var searchCount = 10700;
                    var indexCount = 20;
                    var searchCountPerThread = Convert.ToInt32(searchCount / searchThreadCount);
                    var indexCountPerThread = Convert.ToInt32(indexCount / indexThreadCount);

                    //spawn a bunch of threads to perform some reading                              
                    var tasks = new List<Task>();

                    Action<UmbracoExamineSearcher> doSearch = (s) =>
                    {
                        try
                        {
                            for (var counter = 0; counter < searchCountPerThread; counter++)
                            {
                                var r = s.Search(s.CreateSearchCriteria().Id(docId).Compile());
                                //Debug.WriteLine("searching tId: {0}, tName: {1}, found: {2}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name, r.Count());
                                Thread.Sleep(50);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("ERROR!! {0}", ex);
                            throw;
                        }
                    };

                    Action<UmbracoContentIndexer> doIndex = (ind) =>
                    {
                        try
                        {
                            //reindex the same node a bunch of times
                            for (var i = 0; i < indexCountPerThread; i++)
                            {
                                var cloned = new XElement(node);
                                cloned.Attribute("id").Value = docId.ToString(CultureInfo.InvariantCulture);
                                //Debug.WriteLine("Indexing {0}", i);
                                ind.ReIndexNode(cloned, IndexTypes.Content);
                                Thread.Sleep(100);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("ERROR!! {0}", ex);
                            throw;
                        }
                    };

                    //searching threads
                    for (var i = 0; i < searchThreadCount; i++)
                    {
                        tasks.Add(Task.Factory.StartNew(() => doSearch(customSearcher), TaskCreationOptions.LongRunning));
                    }
                    //indexing threads
                    for (var i = 0; i < indexThreadCount; i++)
                    {
                        tasks.Add(Task.Factory.StartNew(() => doIndex(customIndexer), TaskCreationOptions.LongRunning));
                    }

                    try
                    {
                        Task.WaitAll(tasks.ToArray());
                    }
                    catch (AggregateException e)
                    {
                        var sb = new StringBuilder();
                        sb.Append(e.Message + ": ");
                        foreach (var v in e.InnerExceptions)
                        {
                            sb.Append(v.Message + "; ");
                        }
                        Assert.Fail(sb.ToString());
                    }

                    //we need to check if the indexing is complete
                    while (isIndexing)
                    {
                        //wait until indexing is done
                        Thread.Sleep(1000);
                    }

                    //reset the async mode and remove event handler
                    customIndexer.IndexOptimized -= operationComplete;
                    customIndexer.IndexingError += IndexInitializer.IndexingError;
                    customIndexer.RunAsync = false;

                    //ensure no duplicates
                    Thread.Sleep(10000); //seems to take a while to get its shit together... this i'm not sure why since the optimization should have def finished (and i've stepped through that code!)

                    //var results = customSearcher.Search(customSearcher.CreateSearchCriteria().Id(id).Compile());
                    //Assert.AreEqual(1, results.Count());    
                }
            }
        }

        [Test]
        public void Index_Ensure_No_Duplicates_In_Non_Async()
        {
            //set to 5 so optmization occurs frequently
            _indexer.OptimizationCommitThreshold = 5;

            //get a node from the data repo
            var node = _contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .First();

            //get the id for th node we're re-indexing.
            var id = (int)node.Attribute("id");

            //reindex the same node a bunch of times
            for (var i = 0; i < 29; i++)
            {
                _indexer.ReIndexNode(node, IndexTypes.Content);
            }
           
            //ensure no duplicates
            var results = _searcher.Search(_searcher.CreateSearchCriteria().Id(id).Compile());
            Assert.AreEqual(1, results.Count());
        }     

        [Test]
        public void Index_Rebuild_Index()
        {

            //get searcher and reader to get stats
            var r = ((IndexSearcher)_searcher.GetSearcher()).GetIndexReader();   
                                    
            //there's 16 fields in the index, but 3 sorted fields
            var fields = r.GetFieldNames(IndexReader.FieldOption.ALL);

            Assert.AreEqual(21, fields.Count());
            //ensure there's 3 sorting fields
            Assert.AreEqual(4, fields.Count(x => x.StartsWith(LuceneIndexer.SortedFieldNamePrefix)));
            //there should be 11 documents (10 content, 1 media)
            Assert.AreEqual(10, r.NumDocs());

            //test for the special fields to ensure they are there:
            Assert.AreEqual(1, fields.Where(x => x == LuceneIndexer.IndexNodeIdFieldName).Count());
            Assert.AreEqual(1, fields.Where(x => x == LuceneIndexer.IndexTypeFieldName).Count());
            Assert.AreEqual(1, fields.Where(x => x == UmbracoContentIndexer.IndexPathFieldName).Count());
            Assert.AreEqual(1, fields.Where(x => x == UmbracoContentIndexer.NodeTypeAliasFieldName).Count());

        }


        
        #region Private methods and properties

        private readonly TestContentService _contentService = new TestContentService();
        private readonly TestMediaService _mediaService = new TestMediaService();

        private static UmbracoExamineSearcher _searcher;
        private static UmbracoContentIndexer _indexer;

        #endregion

        #region Initialize and Cleanup

	    private Lucene.Net.Store.Directory _luceneDir;

        [TearDown]
	    public void TestTearDown()
        {
            //set back to 100
            _indexer.OptimizationCommitThreshold = 100;
			_luceneDir.Dispose();
        }

        [SetUp]
		public void TestSetup()
        {
			_luceneDir = new RAMDirectory();
			_indexer = IndexInitializer.GetUmbracoIndexer(_luceneDir);
            _indexer.RebuildIndex();
			_searcher = IndexInitializer.GetUmbracoSearcher(_luceneDir);
        }


        #endregion
    }
}
