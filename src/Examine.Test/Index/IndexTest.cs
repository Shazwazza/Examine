using System;
using System.Collections.Concurrent;
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

        [Test]
        public void Can_Add_Multiple_Values_To_Single_Index_Field()
        {
            using (var d = new CustomRAMDirectory())
            using (var customIndexer = IndexInitializer.GetUmbracoIndexer(d))
            {

                EventHandler<DocumentWritingEventArgs> handler = (sender, args) =>
                {
                    args.Document.Add(new Field("headerText", "another value", Field.Store.YES, Field.Index.ANALYZED));
                };
                
                customIndexer.DocumentWriting += handler;

                customIndexer.RebuildIndex();

                customIndexer.DocumentWriting += handler;
                
                var customSearcher = IndexInitializer.GetLuceneSearcher(d);
                var results = customSearcher.Search(customSearcher.CreateSearchCriteria().NodeName("home").Compile());
                Assert.Greater(results.TotalItemCount, 0);
                foreach (var result in results)
                {
                    var vals = result.GetValues("headerText");
                    Assert.AreEqual(2, vals.Count());
                    Assert.AreEqual("another value", vals.ElementAt(1));
                }
            }
        }


        /// <summary>
        /// Ensures that the cancellation is successful when creating a new index while it's currently indexing
        /// </summary>
        [Test]
        public void Can_Overwrite_Index_During_Indexing_Operation()
        {
            using (var d = new CustomRAMDirectory())
            using (var writer = new IndexWriter(d, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29), IndexWriter.MaxFieldLength.LIMITED))
            using (var customIndexer = IndexInitializer.GetUmbracoIndexer(writer))
            using (var customSearcher = IndexInitializer.GetUmbracoSearcher(writer))
            {

                var waitHandle = new ManualResetEvent(false);

                EventHandler operationComplete = (sender, e) =>
                {
                    //signal that we are done
                    waitHandle.Set();
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

                //spawn a bunch of threads to perform some reading                              
                var tasks = new List<Task>();

                //reindex the same node a bunch of times - then while this is running we'll overwrite below
                for (var i = 0; i < 1000; i++)
                {
                    var indexer = customIndexer;
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        //get next id and put it to the back of the list
                        int docId = i;
                        var cloned = new XElement(node);
                        cloned.Attribute("id").Value = docId.ToString(CultureInfo.InvariantCulture);
                        Debug.WriteLine("Indexing {0}", docId);
                        indexer.ReIndexNode(cloned, IndexTypes.Content);
                    }, TaskCreationOptions.LongRunning));
                }

                Thread.Sleep(100);

                //overwrite!
                customIndexer.EnsureIndex(true);

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

                //reset the async mode and remove event handler
                customIndexer.IndexingError += IndexInitializer.IndexingError;
                customIndexer.RunAsync = false;

                //wait until we are done
                waitHandle.WaitOne();

                writer.WaitForMerges();

                //ensure no data since it's a new index
                var results = customSearcher.Search(customSearcher.CreateSearchCriteria().NodeName("Home").Compile());

                //should be less than the total inserted because we overwrote it in the middle of processing
                Debug.WriteLine("TOTAL RESULTS: " + results.TotalItemCount);
                Assert.Less(results.Count(), 1000);
            }
        }

        /// <summary>
        /// This will create a new index queue item for the same ID multiple times to ensure that the 
        /// index does not end up with duplicate entries.
        /// </summary>
        [Test]
        public void Index_Ensure_No_Duplicates_In_Async()
        {
            using (var d = new CustomRAMDirectory())
            using (var writer = new IndexWriter(d, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29), IndexWriter.MaxFieldLength.LIMITED))
            using (var customIndexer = IndexInitializer.GetUmbracoIndexer(writer))
            using (var customSearcher = IndexInitializer.GetUmbracoSearcher(writer))
            {

                var waitHandle = new ManualResetEvent(false);

                EventHandler operationComplete = (sender, e) =>
                {
                    //signal that we are done
                    waitHandle.Set();
                };

                //add the handler for optimized since we know it will be optimized last based on the commit count
                customIndexer.IndexOperationComplete += operationComplete;

                //remove the normal indexing error handler
                customIndexer.IndexingError -= IndexInitializer.IndexingError;

                //run in async mode
                customIndexer.RunAsync = true;

                //get a node from the data repo
                var idQueue = new ConcurrentQueue<int>(Enumerable.Range(1, 3));
                var node = _contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                    .Root
                    .Elements()
                    .First();

                //reindex the same nodes a bunch of times
                for (var i = 0; i < idQueue.Count * 20; i++)
                {
                    //get next id and put it to the back of the list
                    int docId;
                    if (idQueue.TryDequeue(out docId))
                    {
                        idQueue.Enqueue(docId);

                        var cloned = new XElement(node);
                        cloned.Attribute("id").Value = docId.ToString(CultureInfo.InvariantCulture);
                        Debug.WriteLine("Indexing {0}", docId);
                        customIndexer.ReIndexNode(cloned, IndexTypes.Content);
                        Thread.Sleep(100);
                    }
                }

                //reset the async mode and remove event handler
                customIndexer.IndexingError += IndexInitializer.IndexingError;
                customIndexer.RunAsync = false;
                
                //wait until we are done
                waitHandle.WaitOne();

                writer.WaitForMerges();

                //ensure no duplicates
                
                var results = customSearcher.Search(customSearcher.CreateSearchCriteria().NodeName("Home").Compile());
                Assert.AreEqual(3, results.Count());
            }            
        }

        [Test]
        public void Index_Read_And_Write_Ensure_No_Errors_In_Async()
        {
            using (var d = new CustomRAMDirectory())
            using (var writer = new IndexWriter(d, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29), IndexWriter.MaxFieldLength.LIMITED))
            using (var customIndexer = IndexInitializer.GetUmbracoIndexer(writer))
            using (var customSearcher = IndexInitializer.GetUmbracoSearcher(writer))
            {

                var waitHandle = new ManualResetEvent(false);

                EventHandler operationComplete = (sender, e) =>
                {
                    //signal that we are done
                    waitHandle.Set();
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

                var idQueue = new ConcurrentQueue<int>(Enumerable.Range(1, 10));
                var searchThreadCount = 500;
                var indexThreadCount = 10;
                var searchCount = 10700;
                var indexCount = 100;
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
                            //get next id and put it to the back of the list
                            int docId;
                            if (idQueue.TryDequeue(out docId))
                            {
                                idQueue.Enqueue(docId);
                                var r = s.Search(s.CreateSearchCriteria().Id(docId).Compile());
                                Debug.WriteLine("searching thread: {0}, id: {1}, found: {2}", Thread.CurrentThread.ManagedThreadId, docId, r.Count());
                                Thread.Sleep(50);
                            }
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
                            //get next id and put it to the back of the list
                            int docId;
                            if (idQueue.TryDequeue(out docId))
                            {
                                idQueue.Enqueue(docId);

                                var cloned = new XElement(node);
                                cloned.Attribute("id").Value = docId.ToString(CultureInfo.InvariantCulture);
                                Debug.WriteLine("Indexing {0}", docId);
                                ind.ReIndexNode(cloned, IndexTypes.Content);
                                Thread.Sleep(100);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("ERROR!! {0}", ex);
                        throw;
                    }
                };

                //indexing threads
                for (var i = 0; i < indexThreadCount; i++)
                {
                    var indexer = customIndexer;
                    tasks.Add(Task.Factory.StartNew(() => doIndex(indexer), TaskCreationOptions.LongRunning));
                }

                //searching threads
                for (var i = 0; i < searchThreadCount; i++)
                {
                    var searcher = customSearcher;
                    tasks.Add(Task.Factory.StartNew(() => doSearch(searcher), TaskCreationOptions.LongRunning));
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

                //reset the async mode and remove event handler
                customIndexer.IndexingError += IndexInitializer.IndexingError;
                customIndexer.RunAsync = false;

                //wait until we are done
                waitHandle.WaitOne();

                writer.WaitForMerges();

                var results = customSearcher.Search(customSearcher.CreateSearchCriteria().NodeName("Home").Compile());
                Assert.AreEqual(10, results.Count());
            }
        }

        [Test]
        public void Index_Ensure_No_Duplicates_In_Non_Async()
        {
            using (var d = new CustomRAMDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(d))
            {
                indexer.RebuildIndex();
                var searcher = IndexInitializer.GetUmbracoSearcher(d);

                //set to 5 so optmization occurs frequently
                indexer.OptimizationCommitThreshold = 5;

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
                    indexer.ReIndexNode(node, IndexTypes.Content);
                }

                //ensure no duplicates
                var results = searcher.Search(searcher.CreateSearchCriteria().Id(id).Compile());
                Assert.AreEqual(1, results.Count());
            }
            
            
        }     

        [Test]
        public void Index_Rebuild_Index()
        {
            using (var d = new CustomRAMDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(d))
            {
                indexer.RebuildIndex();
                var searcher = IndexInitializer.GetUmbracoSearcher(d);

                //get searcher and reader to get stats
                var r = ((IndexSearcher)searcher.GetSearcher()).GetIndexReader();

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
        }

        private readonly TestContentService _contentService = new TestContentService();

    }
}
