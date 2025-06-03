using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Examine.Lucene.Analyzers;
using Examine.Lucene.Indexing;
using Examine.Lucene.Providers;
using Examine.Search;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Index
{

    /// <summary>
    /// Tests the standard indexing capabilities
    /// </summary>
    [TestFixture]
    public class LuceneIndexTests : ExamineBaseTest
    {
        [Test]
        public void Operation_Complete_Executes_For_Single_Item()
        {
            using (var d = new RandomIdRAMDirectory())
            using (var writer = new IndexWriter(d, new IndexWriterConfig(LuceneInfo.CurrentVersion, new CultureInvariantStandardAnalyzer())))
            using (var indexer = GetTestIndex(writer))
            {
                var callCount = 0;
                var waitHandle = new ManualResetEvent(false);

                void OperationComplete(object? sender, IndexOperationEventArgs e)
                {
                    callCount++;
                    //signal that we are done
                    waitHandle.Set();
                }

                //add the handler for optimized since we know it will be optimized last based on the commit count
                indexer.IndexOperationComplete += OperationComplete;

                using (indexer.WithThreadingMode(IndexThreadingMode.Asynchronous))
                {
                    var task = Task.Run(() => indexer.IndexItem(new ValueSet(1.ToString(), "content",
                            new Dictionary<string, IEnumerable<object>>
                            {
                                {"item1", new List<object>(new[] {"value1"})},
                                {"item2", new List<object>(new[] {"value2"})}
                            })));

                    // Verify that a single operation calls
                    Task.WaitAll(task);
                    waitHandle.WaitOne(TimeSpan.FromSeconds(30));
                    Assert.AreEqual(1, callCount);
                }
            }
        }

        [Test]
        public void Operation_Complete_Executes_For_Multiple_Items()
        {
            using (var d = new RandomIdRAMDirectory())
            using (var writer = new IndexWriter(d, new IndexWriterConfig(LuceneInfo.CurrentVersion, new CultureInvariantStandardAnalyzer())))
            using (var indexer = GetTestIndex(writer))
            {
                var callCount = 0;
                var waitHandle = new ManualResetEvent(false);

                void OperationComplete(object? sender, IndexOperationEventArgs e)
                {
                    callCount++;

                    if (callCount == 10)
                    {
                        //signal that we are done
                        waitHandle.Set();
                    }
                }

                //add the handler for optimized since we know it will be optimized last based on the commit count
                indexer.IndexOperationComplete += OperationComplete;

                using (indexer.WithThreadingMode(IndexThreadingMode.Asynchronous))
                {
                    var tasks = new List<Task>();
                    for (var i = 0; i < 10; i++)
                    {
                        tasks.Add(Task.Run(() => indexer.IndexItem(new ValueSet(i.ToString(), "content",
                            new Dictionary<string, IEnumerable<object>>
                            {
                                {"item1", new List<object>(new[] {"value1"})},
                                {"item2", new List<object>(new[] {"value2"})}
                            }))));
                    }

                    // Verify that multiple concurrent operations all call
                    Task.WaitAll(tasks.ToArray());
                    waitHandle.WaitOne(TimeSpan.FromSeconds(30));
                    Assert.AreEqual(10, callCount);
                }
            }
        }

        [Test]
        public void Index_Unlocks_When_Disposed()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            {
                Assert.IsFalse(IndexWriter.IsLocked(luceneDir));

                using (var indexer = GetTestIndex(luceneDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
                {
                    indexer.CreateIndex();
                    indexer.IndexItems(TestIndex.AllData());

                    Assert.IsTrue(IndexWriter.IsLocked(luceneDir));
                }

                Assert.IsFalse(IndexWriter.IsLocked(luceneDir));
            }

        }

        [Test]
        public void Rebuild_Index()
        {
            using (var d = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(d, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
            {
                indexer.CreateIndex();
                indexer.IndexItems(TestIndex.AllData());

                var indexWriter = indexer.IndexWriter;
                var reader = indexWriter.IndexWriter.GetReader(true);
                Assert.AreEqual(100, reader.NumDocs);
            }
        }


        [Test]
        public void Index_Exists()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
            {
                indexer.EnsureIndex(true);
                Assert.IsTrue(indexer.IndexExists());
            }
        }

        [Test]
        public void Can_Add_One_Document()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
            {


                indexer.IndexItem(new ValueSet(1.ToString(), "content",
                    new Dictionary<string, IEnumerable<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new[] {"value2"})}
                    }));

                var indexWriter = indexer.IndexWriter;
                var reader = indexWriter.IndexWriter.GetReader(true);
                Assert.AreEqual(1, reader.NumDocs);
            }
        }

        [Test]
        public void Can_Add_Same_Document_Twice_Without_Duplication()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
            {


                var value = new ValueSet(1.ToString(), "content",
                    new Dictionary<string, IEnumerable<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new[] {"value2"})}
                    });

                indexer.IndexItem(value);
                indexer.IndexItem(value);

                var indexWriter = indexer.IndexWriter;
                var reader = indexWriter.IndexWriter.GetReader(true);
                Assert.AreEqual(1, reader.NumDocs);
            }
        }

        [Test]
        public void Can_Add_Multiple_Docs()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
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

                var indexWriter = indexer.IndexWriter;
                var reader = indexWriter.IndexWriter.GetReader(true);
                Assert.AreEqual(10, reader.NumDocs);
            }
        }

        [Test]
        public void Can_Delete()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
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

                var found = indexer.Searcher.CreateQuery().Id("9").Execute();
                Assert.AreEqual(1, found.TotalItemCount);

                indexer.DeleteFromIndex("9");

                found = indexer.Searcher.CreateQuery().Id("9").Execute();
                Assert.AreEqual(0, found.TotalItemCount);

                var indexWriter = indexer.IndexWriter;
                using (var reader = indexWriter.IndexWriter.GetReader(true))
                {
                    Assert.AreEqual(9, reader.NumDocs);
                }
            }
        }


        [Test]
        public void Can_Add_Doc_With_Fields()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
            {


                indexer.IndexItem(new ValueSet(1.ToString(), "content", "test",
                    new Dictionary<string, IEnumerable<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new[] {"value2"})}
                    }));

                var s = (LuceneSearcher)indexer.Searcher;
                var searchContext = s.GetSearchContext();
                using (var searchRef = searchContext.GetSearcher())
                {
                    var luceneSearcher = searchRef.IndexSearcher;

                    var fields = luceneSearcher.Doc(0).Fields.ToArray();

                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item1"));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item2"));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == ExamineFieldNames.ItemTypeFieldName));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == ExamineFieldNames.ItemIdFieldName));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == ExamineFieldNames.CategoryFieldName));
                    Assert.AreEqual("value1", fields.Single(x => x.Name == "item1").GetStringValue());
                    Assert.AreEqual("value2", fields.Single(x => x.Name == "item2").GetStringValue());
                    Assert.AreEqual("test", fields.Single(x => x.Name == ExamineFieldNames.ItemTypeFieldName).GetStringValue());
                    Assert.AreEqual("1", fields.Single(x => x.Name == ExamineFieldNames.ItemIdFieldName).GetStringValue());
                    Assert.AreEqual("content", fields.Single(x => x.Name == ExamineFieldNames.CategoryFieldName).GetStringValue());

                }
            }
        }

        [Test]
        public void Can_Add_Doc_With_Easy_Fields()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
            {


                indexer.IndexItem(ValueSet.FromObject(1.ToString(), "content",
                    new { item1 = "value1", item2 = "value2" }));

                var s = (LuceneSearcher)indexer.Searcher;
                var searchContext = s.GetSearchContext();
                using (var searchRef = searchContext.GetSearcher())
                {
                    var luceneSearcher = searchRef.IndexSearcher;
                    var fields = luceneSearcher.Doc(0).Fields.ToArray();
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item1"));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item2"));
                    Assert.AreEqual("value1", fields.Single(x => x.Name == "item1").GetStringValue());
                    Assert.AreEqual("value2", fields.Single(x => x.Name == "item2").GetStringValue());
                }
            }
        }

        [Test]
        public void Can_Manipulate_ValueSet_In_TransformingIndexValues_Event()
        {
            void AddData(object sender, IndexingItemEventArgs e, string key, string value)
            {
                var updatedValues = e.ValueSet.Values.ToDictionary(x => x.Key, x => x.Value.ToList());

                updatedValues[key] = new List<object>() { value };

                e.SetValues(updatedValues.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value));
            }

            void RemoveData(object sender, IndexingItemEventArgs e, string key)
            {
                var updatedValues = e.ValueSet.Values.ToDictionary(x => x.Key, x => x.Value.ToList());

                updatedValues.Remove(key);

                e.SetValues(updatedValues.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value));
            }

            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
            {

                indexer.TransformingIndexValues += (sender, e) => AddData(sender!, e, "newItem1", "value1");
                indexer.TransformingIndexValues += (sender, e) => RemoveData(sender!, e, "item1");

                indexer.IndexItem(ValueSet.FromObject(1.ToString(), "content",
                    new { item1 = "value1" }));

                var s = (LuceneSearcher)indexer.Searcher;
                var searchContext = s.GetSearchContext();
                using (var searchRef = searchContext.GetSearcher())
                {
                    var luceneSearcher = searchRef.IndexSearcher;
                    var fields = luceneSearcher.Doc(0).Fields.ToArray();
                    Assert.IsNull(fields.SingleOrDefault(x => x.Name == "item1"));
                    Assert.AreEqual("value1", fields.Single(x => x.Name == "newItem1").GetStringValue());
                }
            }
        }




        [Test]
        public void Can_Have_Multiple_Values_In_Fields()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
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

                var s = (LuceneSearcher)indexer.Searcher;
                var searchContext = s.GetSearchContext();
                using (var searchRef = searchContext.GetSearcher())
                {
                    var luceneSearcher = searchRef.IndexSearcher;
                    var fields = luceneSearcher.Doc(0).Fields.ToArray();
                    ;
                    Assert.AreEqual(2, fields.Count(x => x.Name == "item1"));
                    Assert.AreEqual(3, fields.Count(x => x.Name == "item2"));

                    Assert.AreEqual("subval1", fields.Where(x => x.Name == "item1").ElementAt(0).GetStringValue());
                    Assert.AreEqual("subval2", fields.Where(x => x.Name == "item1").ElementAt(1).GetStringValue());

                    Assert.AreEqual("subval1", fields.Where(x => x.Name == "item2").ElementAt(0).GetStringValue());
                    Assert.AreEqual("subval2", fields.Where(x => x.Name == "item2").ElementAt(1).GetStringValue());
                    Assert.AreEqual("subval3", fields.Where(x => x.Name == "item2").ElementAt(2).GetStringValue());
                }
            }
        }

        [Test]
        public void Can_Update_Document()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
            {


                indexer.IndexItem(ValueSet.FromObject(1.ToString(), "content",
                    new { item1 = "value1", item2 = "value2" }));

                indexer.IndexItem(ValueSet.FromObject(1.ToString(), "content",
                    new { item1 = "value3", item2 = "value4" }));

                var s = (LuceneSearcher)indexer.Searcher;
                var searchContext = s.GetSearchContext();
                using (var searchRef = searchContext.GetSearcher())
                {
                    var luceneSearcher = searchRef.IndexSearcher;
                    var fields = luceneSearcher.Doc(luceneSearcher.IndexReader.MaxDoc - 1).Fields.ToArray();
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item1"));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item2"));
                    Assert.AreEqual("value3", fields.Single(x => x.Name == "item1").GetStringValue());
                    Assert.AreEqual("value4", fields.Single(x => x.Name == "item2").GetStringValue());
                }
            }
        }

        [Test]
        public void Number_Field()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                new StandardAnalyzer(LuceneInfo.CurrentVersion),
                new FieldDefinitionCollection(new FieldDefinition("item2", "number"))))
            {


                indexer.IndexItem(new ValueSet(1.ToString(), "content",
                    new Dictionary<string, IEnumerable<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new object[] {123456})}
                    }));

                var s = (LuceneSearcher)indexer.Searcher;
                var searchContext = s.GetSearchContext();
                using (var searchRef = searchContext.GetSearcher())
                {
                    var luceneSearcher = searchRef.IndexSearcher;

                    var fields = luceneSearcher.Doc(luceneSearcher.IndexReader.MaxDoc - 1).Fields.ToArray();

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
            // capture the original console out
            var consoleOut = TestContext.Out;

            void WriteLog(string msg)
            {
                // reset console out to the orig, this is required because we suppress
                // ExecutionContext which is how this is flowed in Nunit so needed when logging
                // in OperationComplete
                Console.SetOut(consoleOut);
                Console.WriteLine(msg);
            }

            const int ThreadCount = 1000;

            using (var d = new RandomIdRAMDirectory())
            using (var writer = new IndexWriter(d, new IndexWriterConfig(LuceneInfo.CurrentVersion, new CultureInvariantStandardAnalyzer())))
            using (var customIndexer = GetTestIndex(writer))
            using (var customSearcher = (LuceneSearcher)customIndexer.Searcher)
            {

                // NOTE: We use this to wait until we're halfway done, and then
                // we will overwrite the index. When we overwrite, the cancelation tokens
                // are canceled which means that any task continuations will also be canceled
                // therefore once the overwrite happens, any current indexing operations will
                // try to continue but none of their continuations will which means the oncomplete
                // callbacks will not execute.

                var middleCompletedWaitHandle = new ManualResetEvent(false);

                var opCompleteCount = 0;
                void OperationComplete(object? sender, IndexOperationEventArgs e)
                {
                    Interlocked.Increment(ref opCompleteCount);

                    WriteLog("OperationComplete: " + opCompleteCount);

                    if (opCompleteCount == ThreadCount / 2)
                    {
                        // signal that we are halfway done
                        WriteLog("HALFWAY!");
                        middleCompletedWaitHandle.Set();
                    }
                }

                //add the handler for completed ops
                customIndexer.IndexOperationComplete += OperationComplete;

                //remove the normal indexing error handler
                customIndexer.IndexingError -= IndexInitializer.IndexingError;

                //run in async mode
                using (customIndexer.WithThreadingMode(IndexThreadingMode.Asynchronous))
                {
                    //get a node from the data repo
                    var node = _contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                        .Root!
                        .Elements()
                        .First();

                    //get the id for th node we're re-indexing.
                    var id = (int)node.Attribute("id")!;

                    //spawn a bunch of threads to perform some reading
                    var tasks = new List<Task>();

                    var rand = new Random(DateTime.Now.Second);

                    //index a node a bunch of times - then while this is running we'll overwrite below
                    for (var i = 0; i < ThreadCount; i++)
                    {
                        var indexer = customIndexer;
                        var docId = i + 1;
                        tasks.Add(Task.Run(() =>
                        {
                            // mimic a slower machine
                            Thread.Sleep(rand.Next(0, 20));
                            //get next id and put it to the back of the list
                            var cloned = new XElement(node);
                            cloned.SetAttributeValue("id", docId);
                            WriteLog("Indexing " + docId);
                            indexer.IndexItem(cloned.ConvertToValueSet(IndexTypes.Content));
                        }));
                    }

                    // wait till we're halfway done
                    middleCompletedWaitHandle.WaitOne();

                    // and then overwrite!
                    WriteLog("Overwriting....");
                    customIndexer.EnsureIndex(true);
                    WriteLog("Done!");

                    try
                    {
                        WriteLog("Waiting on tasks...");
                        Task.WaitAll(tasks.ToArray());
                        WriteLog("Done!");
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
                }

                //reset the async mode and remove event handler
                customIndexer.IndexingError += IndexInitializer.IndexingError;

                customIndexer.WaitForChanges();

                //ensure no data since it's a new index
                var results = customSearcher.CreateQuery()
                    .Field("nodeName", (IExamineValue)new ExamineValue(Examineness.Explicit, "Home"))
                    .Execute();

                // there will be less than the thread count because we overwrote it midway through
                WriteLog("TOTAL RESULTS: " + results.TotalItemCount);
                Assert.Less(results.Count(), ThreadCount);
            }
        }

        /// <summary>
        /// This will create a new index queue item for the same ID multiple times to ensure that the
        /// index does not end up with duplicate entries.
        /// </summary>
        [Test]
        public void Index_Ensure_No_Duplicates_In_Async()
        {
            var rand = new Random(DateTime.Now.Second);

            using (var d = new RandomIdRAMDirectory())
            using (var writer = new IndexWriter(d, new IndexWriterConfig(LuceneInfo.CurrentVersion, new CultureInvariantStandardAnalyzer())))
            using (var customIndexer = GetTestIndex(writer))
            {
                var waitHandle = new ManualResetEvent(false);

                void OperationComplete(object? sender, IndexOperationEventArgs e)
                {
                    //signal that we are done
#pragma warning disable IDE0058 // Expression value is never used
#pragma warning disable IDE0061 // Use expression body for local function
                    waitHandle.Set();
#pragma warning restore IDE0061 // Use expression body for local function
#pragma warning restore IDE0058 // Expression value is never used
                }

                //add the handler for optimized since we know it will be optimized last based on the commit count
                customIndexer.IndexOperationComplete += OperationComplete;

                //remove the normal indexing error handler
                customIndexer.IndexingError -= IndexInitializer.IndexingError;

                //run in async mode
                using (customIndexer.WithThreadingMode(IndexThreadingMode.Asynchronous))
                {
                    //get a node from the data repo
                    var idQueue = new ConcurrentQueue<int>(Enumerable.Range(1, 3));
                    var node = _contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                        .Root!
                        .Elements()
                        .First();

                    //reindex the same nodes a bunch of times
                    for (var i = 0; i < idQueue.Count * 20; i++)
                    {
                        //get next id and put it to the back of the list
                        if (idQueue.TryDequeue(out var docId))
                        {
                            idQueue.Enqueue(docId);

                            Thread.Sleep(rand.Next(0, 100));

                            var cloned = new XElement(node);
                            cloned.Attribute("id")!.Value = docId.ToString(CultureInfo.InvariantCulture);
                            Console.WriteLine("Indexing {0}", docId);
                            customIndexer.IndexItems(new[] { cloned.ConvertToValueSet(IndexTypes.Content) });
                        }
                    }
                }

                //reset the async mode and remove event handler
                customIndexer.IndexingError += IndexInitializer.IndexingError;

                //wait until we are done
                waitHandle.WaitOne();

                //ensure no duplicates

                var customSearcher = (LuceneSearcher)customIndexer.Searcher;
                var results = customSearcher.CreateQuery().Field("nodeName", (IExamineValue)new ExamineValue(Examineness.Explicit, "Home")).Execute();

                foreach (var r in results)
                {
                    Console.WriteLine($"Result Id: {r.Id}");
                }

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

            // capture the original console out
            var consoleOut = TestContext.Out;

            void WriteLog(string msg)
            {
                // reset console out to the orig, this is required because we suppress
                // ExecutionContext which is how this is flowed in Nunit so needed when logging
                // in OperationComplete
                Console.SetOut(consoleOut);
                Console.WriteLine(msg);
            }

            DirectoryInfo? temp = null;
            global::Lucene.Net.Store.Directory directory;
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
                directory = FSDirectory.Open(temp);
            }
            try
            {
                using (var d = directory)
                using (var writer = new IndexWriter(d,
                    new IndexWriterConfig(LuceneInfo.CurrentVersion, new CultureInvariantStandardAnalyzer())))
                using (var customIndexer = GetTestIndex(writer, nrtTargetMaxStaleSec: 1.0, nrtTargetMinStaleSec: 0.1))
                using (var customSearcher = (LuceneSearcher)customIndexer.Searcher)
                using (customIndexer.WithThreadingMode(IndexThreadingMode.Asynchronous))
                {
                    customIndexer.IndexCommitted += (sender, e) => WriteLog("index committed!!!!!!!!!!!!!");

                    var waitHandle = new ManualResetEvent(false);

                    // TODO: This seems broken - we wan see many operations complete while we are indexing/searching
                    // but currently it seems like we are doing all indexing in a single Task which means we only end up
                    // committing once and then Boom, all searches are available, we want to be able to see search results
                    // more immediately.
                    void OperationComplete(object? sender, IndexOperationEventArgs e)
                    {
                        //signal that we are done
#pragma warning disable IDE0058 // Expression value is never used
#pragma warning disable IDE0061 // Use expression body for local function
                        waitHandle.Set();
#pragma warning restore IDE0061 // Use expression body for local function
#pragma warning restore IDE0058 // Expression value is never used
                    }

                    //add the handler for optimized since we know it will be optimized last based on the commit count
                    customIndexer.IndexOperationComplete += OperationComplete;

                    //remove the normal indexing error handler
                    //customIndexer.IndexingError -= IndexInitializer.IndexingError;

                    //get all nodes
                    var nodes = _contentService.GetPublishedContentByXPath("//*[@isDoc]")
                        .Root!
                        .Elements()
                        .ToList();

#pragma warning disable IDE0053 // Use expression body for lambda expression
                    Func<int, XElement> getNode = (index) =>
                    {
                        // clone it
                        return new XElement(nodes[index]);
                    };
#pragma warning restore IDE0053 // Use expression body for lambda expression

                    // we know there are 20 documents available, this is important for the getNode call
                    var idQueue = new ConcurrentQueue<int>(Enumerable.Range(1, 20));

                    var searchCountPerThread = Convert.ToInt32(searchCount / searchThreadCount);
                    var indexCountPerThread = Convert.ToInt32(indexCount / indexThreadCount);

                    //spawn a bunch of threads to perform some reading
                    var tasks = new List<Task>();

                    void doSearch(ISearcher s)
                    {
                        try
                        {
                            for (var counter = 0; counter < searchCountPerThread; counter++)
                            {
                                //get next id and put it to the back of the list
                                if (idQueue.TryDequeue(out var docId))
                                {
                                    idQueue.Enqueue(docId);
                                    var r = s.CreateQuery().Id(docId.ToString()).Execute();
                                    WriteLog(string.Format("searching thread: {0}, id: {1}, found: {2}", Thread.CurrentThread.ManagedThreadId, docId, r.Count()));
                                    Thread.Sleep(searchThreadWait);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog($"Search ERROR!! {ex}");
                            throw;
                        }
                    }

                    void doIndex(IIndex ind)
                    {
                        try
                        {
                            //reindex a nodes a bunch of times
                            for (var i = 0; i < indexCountPerThread; i++)
                            {
                                //get next id and put it to the back of the list
                                if (idQueue.TryDequeue(out var docId))
                                {
                                    idQueue.Enqueue(docId);

                                    var node = getNode(docId - 1);
                                    node.Attribute("id")!.Value = docId.ToString(CultureInfo.InvariantCulture);
                                    WriteLog(string.Format("Indexing {0}", docId));
                                    ind.IndexItems(new[] { node.ConvertToValueSet(IndexTypes.Content) });
                                    Thread.Sleep(indexThreadWait);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog(string.Format("Index ERROR!! {0}", ex));
                            throw;
                        }
                    }

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

                    // At this point we want to guarantee our search will be
                    // for the latest generation. It is possible to do this since
                    // we are using all of the correct NRT implementations.
                    customIndexer.WaitForChanges();

                    var results = customSearcher.CreateQuery().All().Execute();
                    Assert.AreEqual(20, results.Count(), string.Join(", ", results.Select(x => x.Id)));

                    //wait until we are done
                    waitHandle.WaitOne();

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
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not delete temp folder {0}", ex);
                    }
                }
            }
        }



        private readonly TestContentService _contentService = new TestContentService();

    }
}
