using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.QueryParsers.Flexible.Standard;
using Lucene.Net.QueryParsers.Flexible.Standard.Config;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;
using Microsoft.VSDiagnostics;

[assembly: Config(typeof(MyDefaultConfig))]

internal class MyDefaultConfig : ManualConfig
{
    public MyDefaultConfig()
    {
        WithOptions(ConfigOptions.DisableOptimizationsValidator);
    }
}

namespace Examine.Test.Examine.Lucene.Search
{

    /*
| Method          | ThreadCount | Mean       | Error       | StdDev     | Completed Work Items | Lock Contentions | Gen0      | Gen1      | Gen2      | Allocated |
|---------------- |------------ |-----------:|------------:|-----------:|---------------------:|-----------------:|----------:|----------:|----------:|----------:|
|---------------- |------------ |-----------:|------------:|-----------:|---------------------:|-----------------:|----------:|----------:|----------:|----------:|
| ExamineStandard | 1           |   8.712 ms |   0.6798 ms |  0.0373 ms |               1.0000 |                - |  234.3750 |  140.6250 |         - |   2.86 MB |
| LuceneSimple    | 1           |   9.723 ms |   0.4864 ms |  0.0267 ms |               1.0000 |           0.0469 |  250.0000 |  234.3750 |         - |   3.01 MB |
| ExamineStandard | 5           | 154.451 ms |  39.5553 ms |  2.1682 ms |               5.0000 |                - | 1000.0000 |  750.0000 |         - |   14.3 MB |
| LuceneSimple    | 5           |  16.953 ms |   6.1768 ms |  0.3386 ms |               5.0000 |                - | 1250.0000 | 1000.0000 |   93.7500 |  15.06 MB |
| ExamineStandard | 15          | 657.503 ms | 195.5415 ms | 10.7183 ms |              15.0000 |                - | 3000.0000 | 1000.0000 |         - |  42.92 MB |
| LuceneSimple    | 15          |  60.278 ms | 100.6474 ms |  5.5168 ms |              15.0000 |                - | 4333.3333 | 2666.6667 | 1000.0000 |   45.2 MB |


| Method          | ThreadCount | NrtTargetMaxStaleSec | NrtTargetMinStaleSec | Mean      | Error        | StdDev    | Gen0      | Completed Work Items | Lock Contentions | Gen1      | Allocated |
|---------------- |------------ |--------------------- |--------------------- |----------:|-------------:|----------:|----------:|---------------------:|-----------------:|----------:|----------:|
|---------------- |------------ |--------------------- |--------------------- |----------:|-------------:|----------:|----------:|---------------------:|-----------------:|----------:|----------:|
| ExamineStandard | 1           | 5                    | 0.1                  |  13.20 ms |     6.540 ms |  0.358 ms |  250.0000 |               1.0000 |           0.0313 |  156.2500 |   3.13 MB |
| ExamineStandard | 1           | 5                    | 1                    |  12.91 ms |     4.004 ms |  0.219 ms |  250.0000 |               1.0000 |           0.0313 |  156.2500 |   3.13 MB |
| ExamineStandard | 1           | 5                    | 5                    |  13.12 ms |     5.002 ms |  0.274 ms |  250.0000 |               1.0000 |           0.0313 |  156.2500 |   3.13 MB |
| ExamineStandard | 1           | 30                   | 0.1                  |  13.00 ms |     2.902 ms |  0.159 ms |  250.0000 |               1.0000 |                - |  156.2500 |   3.13 MB |
| ExamineStandard | 1           | 30                   | 1                    |  12.99 ms |     1.982 ms |  0.109 ms |  250.0000 |               1.0000 |                - |  156.2500 |   3.13 MB |
| ExamineStandard | 1           | 30                   | 5                    |  13.12 ms |     4.763 ms |  0.261 ms |  250.0000 |               1.0000 |                - |  156.2500 |   3.13 MB |
| ExamineStandard | 1           | 60                   | 0.1                  |  12.94 ms |     4.190 ms |  0.230 ms |  250.0000 |               1.0000 |           0.0313 |  156.2500 |   3.13 MB |
| ExamineStandard | 1           | 60                   | 1                    |  13.17 ms |     4.026 ms |  0.221 ms |  250.0000 |               1.0000 |           0.0781 |  140.6250 |   3.13 MB |
| ExamineStandard | 1           | 60                   | 5                    |  13.59 ms |     7.591 ms |  0.416 ms |  250.0000 |               1.0000 |           0.0625 |  156.2500 |   3.13 MB |
| ExamineStandard | 5           | 5                    | 0.1                  | 164.78 ms |   146.379 ms |  8.024 ms | 1000.0000 |               5.0000 |           4.0000 |  666.6667 |   14.7 MB |
| ExamineStandard | 5           | 5                    | 1                    | 155.77 ms |   173.985 ms |  9.537 ms | 1000.0000 |               5.0000 |           4.0000 |  750.0000 |   14.7 MB |
| ExamineStandard | 5           | 5                    | 5                    | 154.61 ms |   184.531 ms | 10.115 ms | 1000.0000 |               5.0000 |           4.0000 |  666.6667 |   14.7 MB |
| ExamineStandard | 5           | 30                   | 0.1                  | 159.31 ms |   100.583 ms |  5.513 ms | 1000.0000 |               5.0000 |           4.0000 |  750.0000 |   14.7 MB |
| ExamineStandard | 5           | 30                   | 1                    | 157.80 ms |    79.096 ms |  4.336 ms | 1000.0000 |               5.0000 |           4.0000 |  666.6667 |   14.7 MB |
| ExamineStandard | 5           | 30                   | 5                    | 164.48 ms |   171.208 ms |  9.384 ms | 1000.0000 |               5.0000 |           4.0000 |  666.6667 |   14.7 MB |
| ExamineStandard | 5           | 60                   | 0.1                  | 166.63 ms |   163.111 ms |  8.941 ms | 1000.0000 |               5.0000 |           4.0000 |  666.6667 |   14.7 MB |
| ExamineStandard | 5           | 60                   | 1                    | 156.79 ms |   151.734 ms |  8.317 ms | 1000.0000 |               5.0000 |           4.0000 |  750.0000 |   14.7 MB |
| ExamineStandard | 5           | 60                   | 5                    | 160.94 ms |   105.412 ms |  5.778 ms | 1000.0000 |               5.0000 |           4.0000 |  666.6667 |   14.7 MB |
| ExamineStandard | 15          | 5                    | 0.1                  | 661.02 ms | 1,007.163 ms | 55.206 ms | 3000.0000 |              15.0000 |          14.0000 | 1000.0000 |  43.61 MB |
| ExamineStandard | 15          | 5                    | 1                    | 619.05 ms |   282.484 ms | 15.484 ms | 3000.0000 |              15.0000 |          14.0000 | 1000.0000 |  43.61 MB |
| ExamineStandard | 15          | 5                    | 5                    | 615.87 ms |   830.232 ms | 45.508 ms | 3000.0000 |              15.0000 |          14.0000 | 1000.0000 |  43.62 MB |
| ExamineStandard | 15          | 30                   | 0.1                  | 662.71 ms | 1,119.952 ms | 61.388 ms | 3000.0000 |              15.0000 |          14.0000 | 1000.0000 |  43.62 MB |
| ExamineStandard | 15          | 30                   | 1                    | 677.54 ms |   449.274 ms | 24.626 ms | 3000.0000 |              15.0000 |          14.0000 | 1000.0000 |  43.61 MB |
| ExamineStandard | 15          | 30                   | 5                    | 679.11 ms |   963.257 ms | 52.799 ms | 3000.0000 |              15.0000 |          14.0000 | 1000.0000 |  43.62 MB |
| ExamineStandard | 15          | 60                   | 0.1                  | 695.26 ms |   471.371 ms | 25.837 ms | 3000.0000 |              15.0000 |          14.0000 | 1000.0000 |  43.62 MB |
| ExamineStandard | 15          | 60                   | 1                    | 628.51 ms |   421.771 ms | 23.119 ms | 3000.0000 |              15.0000 |          14.0000 | 1000.0000 |  43.61 MB |
| ExamineStandard | 15          | 60                   | 5                    | 706.39 ms |   510.552 ms | 27.985 ms | 3000.0000 |

Without NRT

| Method          | ThreadCount | Mean      | Error      | StdDev    | Gen0      | Completed Work Items | Lock Contentions | Gen1      | Allocated |
|---------------- |------------ |----------:|-----------:|----------:|----------:|---------------------:|-----------------:|----------:|----------:|
| ExamineStandard | 1           |  12.48 ms |   3.218 ms |  0.176 ms |  250.0000 |               1.0000 |           0.0938 |  156.2500 |   3.13 MB |
| ExamineStandard | 5           | 149.31 ms |  88.914 ms |  4.874 ms | 1000.0000 |               5.0000 |           4.0000 |  750.0000 |   14.7 MB |
| ExamineStandard | 15          | 613.14 ms | 897.936 ms | 49.219 ms | 3000.0000 |              15.0000 |          14.0000 | 1000.0000 |  43.67 MB |

Without querying MaxDoc

| Method          | ThreadCount | Mean       | Error     | StdDev    | Gen0     | Completed Work Items | Lock Contentions | Gen1     | Allocated   |
|---------------- |------------ |-----------:|----------:|----------:|---------:|---------------------:|-----------------:|---------:|------------:|
| ExamineStandard | 1           |   5.223 ms |  1.452 ms | 0.0796 ms |  78.1250 |               1.0000 |           0.0313 |   7.8125 |      962 KB |
| ExamineStandard | 5           |  26.772 ms |  9.982 ms | 0.5471 ms | 312.5000 |               5.0000 |           4.0000 | 187.5000 |  3825.35 KB |
| ExamineStandard | 15          | 101.483 ms | 65.690 ms | 3.6007 ms | 800.0000 |              15.0000 |          14.0000 | 400.0000 | 10989.05 KB |

Without apply deletes

| Method          | ThreadCount | Mean       | Error     | StdDev    | Gen0     | Completed Work Items | Lock Contentions | Gen1     | Allocated   |
|---------------- |------------ |-----------:|----------:|----------:|---------:|---------------------:|-----------------:|---------:|------------:|
| ExamineStandard | 1           |   5.554 ms |  1.745 ms | 0.0957 ms |  78.1250 |               1.0000 |                - |  31.2500 |   961.73 KB |
| ExamineStandard | 5           |  26.960 ms |  4.797 ms | 0.2629 ms | 312.5000 |               5.0000 |           4.0313 | 187.5000 |   3826.6 KB |
| ExamineStandard | 15          | 103.939 ms | 49.361 ms | 2.7057 ms | 800.0000 |              15.0000 |          14.0000 | 400.0000 | 10991.87 KB |

Using struct (doesn't change anything)

| Method          | ThreadCount | Mean       | Error     | StdDev    | Gen0     | Completed Work Items | Lock Contentions | Gen1     | Allocated   |
|---------------- |------------ |-----------:|----------:|----------:|---------:|---------------------:|-----------------:|---------:|------------:|
| ExamineStandard | 1           |   5.661 ms |  2.477 ms | 0.1357 ms |  78.1250 |               1.0000 |           0.0625 |  31.2500 |   961.56 KB |
| ExamineStandard | 5           |  28.364 ms |  3.615 ms | 0.1981 ms | 312.5000 |               5.0000 |           4.0000 | 187.5000 |  3825.91 KB |
| ExamineStandard | 15          | 100.561 ms | 26.820 ms | 1.4701 ms | 800.0000 |              15.0000 |          14.0000 | 400.0000 | 10986.15 KB |

|---------------- |------------ |-----------:|----------:|----------:|---------:|---------------------:|-----------------:|---------:|------------:|
| Method          | ThreadCount | Mean       | Error     | StdDev    | Gen0     | Completed Work Items | Lock Contentions | Gen1     | Allocated   |
|---------------- |------------ |-----------:|----------:|----------:|---------:|---------------------:|-----------------:|---------:|------------:|
| ExamineStandard | 1           |   5.471 ms |  1.430 ms | 0.0784 ms |  78.1250 |               1.0000 |           0.0156 |  31.2500 |   958.55 KB |
| ExamineStandard | 5           |  26.521 ms |  1.837 ms | 0.1007 ms | 312.5000 |               5.0000 |           4.0000 | 156.2500 |  3808.24 KB |
| ExamineStandard | 15          | 102.785 ms | 80.640 ms | 4.4202 ms | 833.3333 |              15.0000 |          14.0000 | 500.0000 | 10935.97 KB |

With Latest changes:

| Method          | ThreadCount | Mean       | Error       | StdDev     | Completed Work Items | Lock Contentions | Gen0      | Gen1      | Allocated   |
|---------------- |------------ |-----------:|------------:|-----------:|---------------------:|-----------------:|----------:|----------:|------------:|
| ExamineStandard | 1           |   5.157 ms |   1.0374 ms |  0.0569 ms |               1.0000 |           0.0156 |   78.1250 |   39.0625 |    963.3 KB |
| LuceneSimple    | 1           |  11.338 ms |   0.8416 ms |  0.0461 ms |               1.0000 |           0.0156 |  265.6250 |  187.5000 |  3269.09 KB |
| ExamineStandard | 5           |  27.038 ms |   7.2847 ms |  0.3993 ms |               5.0000 |           4.0000 |  312.5000 |  187.5000 |   3812.7 KB |
| LuceneSimple    | 5           | 144.196 ms | 185.2203 ms | 10.1526 ms |               5.0000 |                - | 1000.0000 |  750.0000 | 15047.06 KB |
| ExamineStandard | 15          |  95.799 ms |  64.1371 ms |  3.5156 ms |              15.0000 |          14.0000 |  833.3333 |  500.0000 | 10940.31 KB |
| LuceneSimple    | 15          | 566.652 ms | 275.2278 ms | 15.0862 ms |              15.0000 |                - | 3000.0000 | 1000.0000 |  44485.6 KB |

     */
    [ShortRunJob]
    [ThreadingDiagnoser]
    [MemoryDiagnoser]
    [DotNetCountersDiagnoser]
    [CPUUsageDiagnoser]
    public class ConcurrentSearchBenchmarks : ExamineBaseTest
    {
        private readonly StandardAnalyzer _analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
        private ILogger<ConcurrentSearchBenchmarks> _logger;
        private string _tempBasePath;
        private FSDirectory _luceneDir;

        [GlobalSetup]
        public override void Setup()
        {
            base.Setup();

            _logger = LoggerFactory.CreateLogger<ConcurrentSearchBenchmarks>();
            _tempBasePath = Path.Combine(Path.GetTempPath(), "ExamineTests");

            var tempPath = Path.Combine(_tempBasePath, Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(tempPath);
            var temp = new DirectoryInfo(tempPath);
            _luceneDir = FSDirectory.Open(temp);
            using var indexer = GetTestIndex(_luceneDir, _analyzer);

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
        }

        [GlobalCleanup]
        public override void TearDown()
        {
            _luceneDir.Dispose();
            _analyzer.Dispose();

            base.TearDown();

            System.IO.Directory.Delete(_tempBasePath, true);
        }

        [Params(1, 5, 15)]
        public int ThreadCount { get; set; }

        //[Params(5, 30, 60)]
        //public double NrtTargetMaxStaleSec { get; set; }

        //[Params(0.1, 1, 5)]
        //public double NrtTargetMinStaleSec { get; set; }

        [Benchmark]
        public async Task ExamineStandard()
        {
            using var indexer = GetTestIndex(
                _luceneDir,
                _analyzer,
                nrtEnabled: true);
                ////nrtTargetMaxStaleSec: NrtTargetMaxStaleSec,
                ////nrtTargetMinStaleSec: NrtTargetMinStaleSec);

            var tasks = new List<Task>();

            for (var i = 0; i < ThreadCount; i++)
            {
                tasks.Add(new Task(() =>
                {
                    // always resolve the searcher from the indexer
                    var searcher = indexer.Searcher;

                    var query = searcher.CreateQuery("content").Field("nodeName", "location".MultipleCharacterWildcard());
                    var results = query.Execute();

                    // enumerate (forces the result to execute)
                    var logOutput = "ThreadID: " + Thread.CurrentThread.ManagedThreadId + ", Results: " + string.Join(',', results.Select(x => $"{x.Id}-{x.Values.Count}-{x.Score}").ToArray());
                    _logger.LogDebug(logOutput);
                }));
            }

            foreach (var task in tasks)
            {
                task.Start();
            }

            await Task.WhenAll(tasks);
        }

        [Benchmark]
        public async Task LuceneSimple()
        {
            var tasks = new List<Task>();

            using var dirReader = DirectoryReader.Open(_luceneDir);
            using var writer = new IndexWriter(dirReader.Directory, new IndexWriterConfig(LuceneVersion.LUCENE_48, _analyzer));
            var trackingWriter = new TrackingIndexWriter(writer);
            using var searcherManager = new SearcherManager(trackingWriter.IndexWriter, applyAllDeletes: false, new SearcherFactory());

            for (var i = 0; i < ThreadCount; i++)
            {
                tasks.Add(new Task(() =>
                {
                    var parser = new QueryParser(LuceneVersion.LUCENE_48, ExamineFieldNames.ItemIdFieldName, new StandardAnalyzer(LuceneVersion.LUCENE_48));
                    var query = parser.Parse($"{ExamineFieldNames.CategoryFieldName}:content AND nodeName:location*");

                    // this is like doing Acquire
                    using var context = searcherManager.GetContext();

                    var searcher = context.Reference;

                    var maxDoc = searcher.IndexReader.MaxDoc;
                    var topDocsCollector = TopScoreDocCollector.Create(maxDoc, null, true);

                    searcher.Search(query, topDocsCollector);
                    var topDocs = ((TopScoreDocCollector)topDocsCollector).GetTopDocs(0, QueryOptions.DefaultMaxResults);

                    var totalItemCount = topDocs.TotalHits;

                    var results = new List<ISearchResult>(topDocs.ScoreDocs.Length);
                    for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
                    {
                        var scoreDoc = topDocs.ScoreDocs[i];
                        var docId = scoreDoc.Doc;
                        var doc = searcher.Doc(docId);
                        var score = scoreDoc.Score;
                        var shardIndex = scoreDoc.ShardIndex;
                        var result = LuceneSearchExecutor.CreateSearchResult(doc, score, shardIndex);
                        results.Add(result);
                    }
                    var searchAfterOptions = LuceneSearchExecutor.GetSearchAfterOptions(topDocs);
                    float maxScore = topDocs.MaxScore;

                    // enumerate (forces the result to execute)
                    var logOutput = "ThreadID: " + Thread.CurrentThread.ManagedThreadId + ", Results: " + string.Join(',', results.Select(x => $"{x.Id}-{x.Values.Count}-{x.Score}").ToArray());
                    _logger.LogDebug(logOutput);
                }));
            }

            foreach (var task in tasks)
            {
                task.Start();
            }

            await Task.WhenAll(tasks);
        }

        protected override ILoggerFactory CreateLoggerFactory()
            => Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
    }
}
