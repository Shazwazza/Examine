using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Search
{
    [TestFixture]
    public class ConcurrencyTests : ExamineBaseTest
    {
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(25)]
        public async Task BenchmarkConcurrentSearching(int threads)
        {
            var tempBasePath = Path.Combine(Path.GetTempPath(), "ExamineTests");

            try
            {
                var tempPath = Path.Combine(tempBasePath, Guid.NewGuid().ToString());
                System.IO.Directory.CreateDirectory(tempPath);
                var temp = new DirectoryInfo(tempPath);

                var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
                using (var luceneDir = FSDirectory.Open(temp))
                using (var indexer = GetTestIndex(
                    luceneDir,
                    analyzer))
                {
                    indexer.IndexItems(new[] {
                        ValueSet.FromObject(1.ToString(), "content",
                            new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                        ValueSet.FromObject(2.ToString(), "content",
                            new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                        ValueSet.FromObject(3.ToString(), "content",
                            new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia"})
                    });

                    var tasks = new List<Task>();

                    for (int i = 0; i < threads; i++)
                    {
                        tasks.Add(new Task(() =>
                        {
                            // always resolve the searcher from the indexer
                            var searcher = indexer.Searcher;

                            var query = searcher.CreateQuery("content").Field("nodeName", "location".MultipleCharacterWildcard());
                            var results = query.Execute();

                            // enumerate (forces the result to execute)
                            Console.WriteLine("ThreadID: " + Thread.CurrentThread.ManagedThreadId + ", Results: " + string.Join(',', results.Select(x => $"{x.Id}-{x.Values.Count}-{x.Score}").ToArray()));
                        }));
                    }

                    var stopwatch = new Stopwatch();

                    try
                    {
                        stopwatch.Start();
                        foreach (var task in tasks)
                        {
                            task.Start();
                        }

                        await Task.WhenAll(tasks);
                    }
                    finally
                    {
                        stopwatch.Stop();
                        Console.WriteLine("Completed in ms: " + stopwatch.ElapsedMilliseconds);
                    }
                }
            }
            finally
            {
                System.IO.Directory.Delete(tempBasePath, true);
            }
        }
    }
}
