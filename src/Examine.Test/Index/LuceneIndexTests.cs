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
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Search;
using Examine.Search;
using Examine.Test.DataServices;
using Examine.Test.UmbracoExamine;
using Version = Lucene.Net.Util.Version;

namespace Examine.Test.Index
{

    /// <summary>
    /// Tests the standard indexing capabilities
    /// </summary>
    [TestFixture]
    public class LuceneIndexTests
    {
        [Test]
        public void Rebuild_Index()
        {
            using (var d = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(d, new StandardAnalyzer(Version.LUCENE_30)))
            {
                indexer.CreateIndex();
                indexer.IndexItems(indexer.AllData());

                var indexWriter = indexer.GetIndexWriter();
                var reader = indexWriter.GetReader();
                Assert.AreEqual(100, reader.NumDocs());
            }
        }


        [Test]
        public void Index_Exists()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            {
                indexer.EnsureIndex(true);
                Assert.IsTrue(indexer.IndexExists());
            }
        }

        [Test]
        public void Can_Add_One_Document()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            {


                indexer.IndexItem(new ValueSet(1.ToString(), "content",
                    new Dictionary<string, IEnumerable<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new[] {"value2"})}
                    }));

                var indexWriter = indexer.GetIndexWriter();
                var reader = indexWriter.GetReader();
                Assert.AreEqual(1, reader.NumDocs());
            }
        }

        [Test]
        public void Can_Add_Same_Document_Twice_Without_Duplication()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            {


                var value = new ValueSet(1.ToString(), "content",
                    new Dictionary<string, IEnumerable<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new[] {"value2"})}
                    });

                indexer.IndexItem(value);
                indexer.IndexItem(value);

                var indexWriter = indexer.GetIndexWriter();
                var reader = indexWriter.GetReader();
                Assert.AreEqual(1, reader.NumDocs());
            }
        }

        [Test]
        public void Can_Add_Multiple_Docs()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            {


                for (var i = 0; i < 10; i++)
                {
                    indexer.IndexItem(new ValueSet(i.ToString(), "content",
                        new Dictionary<string, IEnumerable<object>>
                        {
                            {"item1", new List<object>(new[] {"value1"})},
                            {"item2", new List<object>(new[] {"value2"})}
                        }));
                }

                var indexWriter = indexer.GetIndexWriter();
                var reader = indexWriter.GetReader();
                Assert.AreEqual(10, reader.NumDocs());
            }
        }

        [Test]
        public void Can_Delete()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            {


                for (var i = 0; i < 10; i++)
                {
                    indexer.IndexItem(new ValueSet(i.ToString(), "content",
                        new Dictionary<string, IEnumerable<object>>
                        {
                            {"item1", new List<object>(new[] {"value1"})},
                            {"item2", new List<object>(new[] {"value2"})}
                        }));
                }
                indexer.DeleteFromIndex("9");

                var indexWriter = indexer.GetIndexWriter();
                var reader = indexWriter.GetReader();
                Assert.AreEqual(9, reader.NumDocs());
            }
        }


        [Test]
        public void Can_Add_Doc_With_Fields()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            {


                indexer.IndexItem(new ValueSet(1.ToString(), "content", "test",
                    new Dictionary<string, IEnumerable<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new[] {"value2"})}
                    }));


                using (var s = (LuceneSearcher)indexer.GetSearcher())
                {
                    var luceneSearcher = s.GetLuceneSearcher();
                    var fields = luceneSearcher.Doc(0).GetFields().ToArray();
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item1"));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item2"));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == ExamineFieldNames.ItemTypeFieldName));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == ExamineFieldNames.ItemIdFieldName));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == ExamineFieldNames.CategoryFieldName));

                    Assert.AreEqual("value1", fields.Single(x => x.Name == "item1").StringValue);
                    Assert.AreEqual("value2", fields.Single(x => x.Name == "item2").StringValue);
                    Assert.AreEqual("test", fields.Single(x => x.Name == ExamineFieldNames.ItemTypeFieldName).StringValue);
                    Assert.AreEqual("1", fields.Single(x => x.Name == ExamineFieldNames.ItemIdFieldName).StringValue);
                    Assert.AreEqual("content", fields.Single(x => x.Name == ExamineFieldNames.CategoryFieldName).StringValue);
                }
            }
        }

        [Test]
        public void Can_Add_Doc_With_Easy_Fields()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            {


                indexer.IndexItem(ValueSet.FromObject(1.ToString(), "content",
                    new { item1 = "value1", item2 = "value2" }));

                using (var s = (LuceneSearcher)indexer.GetSearcher())
                {
                    var luceneSearcher = s.GetLuceneSearcher();
                    var fields = luceneSearcher.Doc(0).GetFields().ToArray();
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item1"));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item2"));
                    Assert.AreEqual("value1", fields.Single(x => x.Name == "item1").StringValue);
                    Assert.AreEqual("value2", fields.Single(x => x.Name == "item2").StringValue);
                }
            }
        }

        [Test]
        public void Can_Have_Multiple_Values_In_Fields()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            {


                indexer.IndexItem(new ValueSet(1.ToString(), "content",
                    new Dictionary<string, IEnumerable<object>>
                    {
                        {
                            "item1", new List<object> {"subval1", "subval2"}
                        },
                        {
                            "item2", new List<object> {"subval1", "subval2", "subval3"}
                        }
                    }));

                using (var s = (LuceneSearcher)indexer.GetSearcher())
                {
                    var luceneSearcher = s.GetLuceneSearcher();
                    var fields = luceneSearcher.Doc(0).GetFields().ToArray();
                    Assert.AreEqual(2, fields.Count(x => x.Name == "item1"));
                    Assert.AreEqual(3, fields.Count(x => x.Name == "item2"));

                    Assert.AreEqual("subval1", fields.Where(x => x.Name == "item1").ElementAt(0).StringValue);
                    Assert.AreEqual("subval2", fields.Where(x => x.Name == "item1").ElementAt(1).StringValue);

                    Assert.AreEqual("subval1", fields.Where(x => x.Name == "item2").ElementAt(0).StringValue);
                    Assert.AreEqual("subval2", fields.Where(x => x.Name == "item2").ElementAt(1).StringValue);
                    Assert.AreEqual("subval3", fields.Where(x => x.Name == "item2").ElementAt(2).StringValue);
                }
            }
        }

        [Test]
        public void Can_Update_Document()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            {


                indexer.IndexItem(ValueSet.FromObject(1.ToString(), "content",
                    new { item1 = "value1", item2 = "value2" }));

                indexer.IndexItem(ValueSet.FromObject(1.ToString(), "content",
                    new { item1 = "value3", item2 = "value4" }));

                using (var s = (LuceneSearcher)indexer.GetSearcher())
                {
                    var luceneSearcher = s.GetLuceneSearcher();
                    var fields = luceneSearcher.Doc(luceneSearcher.MaxDoc - 1).GetFields().ToArray();
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item1"));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item2"));
                    Assert.AreEqual("value3", fields.Single(x => x.Name == "item1").StringValue);
                    Assert.AreEqual("value4", fields.Single(x => x.Name == "item2").StringValue);
                }
            }
        }

        [Test]
        public void Number_Field()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(
                new FieldDefinitionCollection(new FieldDefinition("item2", "number")),
                luceneDir,
                new StandardAnalyzer(Version.LUCENE_30)))
            {


                indexer.IndexItem(new ValueSet(1.ToString(), "content",
                    new Dictionary<string, IEnumerable<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new object[] {123456})}
                    }));

                using (var s = (LuceneSearcher)indexer.GetSearcher())
                {
                    var luceneSearcher = s.GetLuceneSearcher();
                    var fields = luceneSearcher.Doc(luceneSearcher.MaxDoc - 1).GetFields().ToArray();

                    var valType = indexer.FieldValueTypeCollection.GetValueType("item2");
                    Assert.AreEqual(typeof(Int32Type), valType.GetType());
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item2"));
                }
            }

        }

        /// <summary>
        /// Ensures that the cancellation is successful when creating a new index while it's currently indexing
        /// </summary>
        [Test]
        public void Can_Overwrite_Index_During_Indexing_Operation()
        {
            const int ThreadCount = 1000;

            using (var d = new RandomIdRAMDirectory())
            using (var writer = new IndexWriter(d, new CultureInvariantStandardAnalyzer(Version.LUCENE_30), IndexWriter.MaxFieldLength.LIMITED))
            using (var customIndexer = new TestIndex(writer))
            using (var customSearcher = (LuceneSearcher)customIndexer.GetSearcher())
            {

                var waitHandle = new ManualResetEvent(false);

                var opCompleteCount = 0;
                void OperationComplete(object sender, IndexOperationEventArgs e)
                {
                    Interlocked.Increment(ref opCompleteCount);

                    Console.WriteLine($"OperationComplete: {opCompleteCount}");

                    if (opCompleteCount == ThreadCount)
                    {
                        //signal that we are done
                        waitHandle.Set();
                    }
                }

                //add the handler for optimized since we know it will be optimized last based on the commit count
                customIndexer.IndexOperationComplete += OperationComplete;

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
                for (var i = 0; i < ThreadCount; i++)
                {
                    var indexer = customIndexer;
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        //get next id and put it to the back of the list
                        int docId = i;
                        var cloned = new XElement(node);
                        Console.WriteLine("Indexing {0}", docId);
                        indexer.IndexItem(cloned.ConvertToValueSet(IndexTypes.Content));
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
                waitHandle.WaitOne(TimeSpan.FromMinutes(2));

                writer.WaitForMerges();

                //ensure no data since it's a new index
                var results = customSearcher.CreateQuery().Field("nodeName", (IExamineValue)new ExamineValue(Examineness.Explicit, "Home")).Execute();

                //the total times that OperationComplete event should be fired is 1000
                Assert.AreEqual(1000, opCompleteCount);

                //should be less than the total inserted because we overwrote it in the middle of processing
                Console.WriteLine("TOTAL RESULTS: " + results.TotalItemCount);
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
            using (var d = new RandomIdRAMDirectory())
            using (var writer = new IndexWriter(d, new CultureInvariantStandardAnalyzer(Version.LUCENE_30), IndexWriter.MaxFieldLength.LIMITED))
            using (var customIndexer = new TestIndex(writer))
            //using (var customSearcher = (LuceneSearcher)customIndexer.GetSearcher())
            {

                var waitHandle = new ManualResetEvent(false);

                void OperationComplete(object sender, IndexOperationEventArgs e)
                {
                    //signal that we are done
                    waitHandle.Set();
                }

                //add the handler for optimized since we know it will be optimized last based on the commit count
                customIndexer.IndexOperationComplete += OperationComplete;

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
                        Console.WriteLine("Indexing {0}", docId);
                        customIndexer.IndexItems(new[] { cloned.ConvertToValueSet(IndexTypes.Content) });
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

                var customSearcher = (LuceneSearcher)customIndexer.GetSearcher();
                var results = customSearcher.CreateQuery().Field("nodeName", (IExamineValue)new ExamineValue(Examineness.Explicit, "Home")).Execute();
                Assert.AreEqual(3, results.Count());
            }
        }

        //[TestCase(10000, 100700, 20, 50, 100, 50, true, Explicit = true)]
        [TestCase(500, 2000, 20, 50, 100, 50, false)]
        [TestCase(2000, 5000, 20, 50, 100, 50, true)]
        public void Index_Read_And_Write_Ensure_No_Errors_In_Async(
            int indexCount,
            int searchCount,
            int indexThreadCount,
            int searchThreadCount,
            int indexThreadWait,
            int searchThreadWait,
            bool inMemory)
        {
            // TODO: In this test can we ensure all readers are tracked and closed?
            // TODO: In the search part, we should be searching in various ways and also with skip

            DirectoryInfo temp = null;
            Lucene.Net.Store.Directory directory;
            if (inMemory)
            {
                directory = new RandomIdRAMDirectory();
            }
            else
            {
                // try to clear out old files
                var tempBasePath = Path.Combine(Path.GetTempPath(), "ExamineTests");
                if (System.IO.Directory.Exists(tempBasePath))
                {
                    try
                    {
                        System.IO.Directory.Delete(tempBasePath, true);
                    }
                    catch
                    {
                    }
                }

                var tempPath = Path.Combine(tempBasePath, Guid.NewGuid().ToString());
                System.IO.Directory.CreateDirectory(tempPath);
                temp = new DirectoryInfo(tempPath);
                directory = new SimpleFSDirectory(temp);
            }

            try
            {
                using (var d = directory)
                using (var writer = new IndexWriter(d, new CultureInvariantStandardAnalyzer(Version.LUCENE_30), IndexWriter.MaxFieldLength.LIMITED))
                using (var customIndexer = new TestIndex(writer))
                using (var customSearcher = (LuceneSearcher)customIndexer.GetSearcher())
                {

                    var waitHandle = new ManualResetEvent(false);

                    void OperationComplete(object sender, IndexOperationEventArgs e)
                    {
                        //signal that we are done
                        waitHandle.Set();
                    }

                    //add the handler for optimized since we know it will be optimized last based on the commit count
                    customIndexer.IndexOperationComplete += OperationComplete;

                    //remove the normal indexing error handler
                    //customIndexer.IndexingError -= IndexInitializer.IndexingError;

                    //run in async mode
                    customIndexer.RunAsync = true;

                    //get all nodes
                    var nodes = _contentService.GetPublishedContentByXPath("//*[@isDoc]")
                        .Root
                        .Elements()
                        .ToList();

                    Func<int, XElement> getNode = (index) =>
                    {
                        // clone it
                        return new XElement(nodes[index]);
                    };

                    // we know there are 20 documents available, this is important for the getNode call
                    var idQueue = new ConcurrentQueue<int>(Enumerable.Range(1, 20));

                    var searchCountPerThread = Convert.ToInt32(searchCount / searchThreadCount);
                    var indexCountPerThread = Convert.ToInt32(indexCount / indexThreadCount);

                    //spawn a bunch of threads to perform some reading                              
                    var tasks = new List<Task>();

                    Action<ISearcher> doSearch = (s) =>
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
                                    var r = s.CreateQuery().Id(docId.ToString()).Execute();
                                    Console.WriteLine("searching thread: {0}, id: {1}, found: {2}", Thread.CurrentThread.ManagedThreadId, docId, r.Count());
                                    Thread.Sleep(searchThreadWait);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Search ERROR!! {0}", ex);
                            throw;
                        }
                    };

                    Action<IIndex> doIndex = (ind) =>
                    {
                        try
                        {
                            //reindex a nodes a bunch of times
                            for (var i = 0; i < indexCountPerThread; i++)
                            {
                                //get next id and put it to the back of the list
                                int docId;
                                if (idQueue.TryDequeue(out docId))
                                {
                                    idQueue.Enqueue(docId);

                                    var node = getNode(docId - 1);
                                    node.Attribute("id").Value = docId.ToString(CultureInfo.InvariantCulture);
                                    Console.WriteLine("Indexing {0}", docId);
                                    ind.IndexItems(new[] { node.ConvertToValueSet(IndexTypes.Content) });
                                    Thread.Sleep(indexThreadWait);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Index ERROR!! {0}", ex);
                            throw;
                        }
                    };

                    //indexing threads
                    for (var i = 0; i < indexThreadCount; i++)
                    {
                        var indexer = customIndexer;
                        tasks.Add(Task.Run(() => doIndex(indexer)));
                    }

                    //searching threads
                    for (var i = 0; i < searchThreadCount; i++)
                    {
                        var searcher = customSearcher;
                        tasks.Add(Task.Run(() => doSearch(searcher)));
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

                    var results = customSearcher.CreateQuery().All().Execute();
                    Assert.AreEqual(20, results.Count());

                    //reset the async mode and remove event handler
                    //customIndexer.IndexingError += IndexInitializer.IndexingError;
                    //customIndexer.RunAsync = false;

                    //wait until we are done
                    waitHandle.WaitOne();

                    writer.WaitForMerges();
                    writer.Dispose(true);

                    results = customSearcher.CreateQuery().All().Execute();
                    Assert.AreEqual(20, results.Count());
                }
            }
            finally
            {
                if (temp != null)
                {
                    try
                    {
                        temp.Delete(true);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Could not delete temp folder {0}", ex);
                    }
                }
            }
        }



        private readonly TestContentService _contentService = new TestContentService();

    }
}
