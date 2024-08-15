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
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Search
{
    [TestFixture]
    public class ConcurrencyTests : ExamineBaseTest
    {
        protected override ILoggerFactory CreateLoggerFactory()
            => Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(25)]
        [TestCase(100)]
        public async Task BenchmarkConcurrentSearching(int threads)
        {
            var logger = LoggerFactory.CreateLogger<ConcurrencyTests>();
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
                    var random = new Random();
                    var valueSets = new List<ValueSet>();

                    for (var i = 0; i < 1000; i++)
                    {
                        valueSets.Add(ValueSet.FromObject(Guid.NewGuid().ToString(), "content",
                            new
                            {
                                nodeName = "location " + i,
                                bodyText = Enumerable.Range(0, random.Next(10, 100)).Select(x => Guid.NewGuid().ToString())
                            }));
                    }

                    indexer.IndexItems(valueSets);

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
                            var logOutput = "ThreadID: " + Thread.CurrentThread.ManagedThreadId + ", Results: " + string.Join(',', results.Select(x => $"{x.Id}-{x.Values.Count}-{x.Score}").ToArray());
                            logger.LogDebug(logOutput);
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
                        logger.LogInformation("Completed in ms: " + stopwatch.ElapsedMilliseconds);
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
