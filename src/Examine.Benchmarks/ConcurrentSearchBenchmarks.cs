using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Examine.Lucene.Search;
using Examine.Search;
using Examine.Test;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
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

namespace Examine.Benchmarks
{
    /*

     Original

    | Method          | ThreadCount | Mean       | Error       | StdDev     | Completed Work Items | Lock Contentions | Gen0      | Gen1      | Gen2      | Allocated |
    |---------------- |------------ |-----------:|------------:|-----------:|---------------------:|-----------------:|----------:|----------:|----------:|----------:|
    |---------------- |------------ |-----------:|------------:|-----------:|---------------------:|-----------------:|----------:|----------:|----------:|----------:|
    | ExamineStandard | 1           |   8.712 ms |   0.6798 ms |  0.0373 ms |               1.0000 |                - |  234.3750 |  140.6250 |         - |   2.86 MB |
    | LuceneSimple    | 1           |   9.723 ms |   0.4864 ms |  0.0267 ms |               1.0000 |           0.0469 |  250.0000 |  234.3750 |         - |   3.01 MB |
    | ExamineStandard | 5           | 154.451 ms |  39.5553 ms |  2.1682 ms |               5.0000 |                - | 1000.0000 |  750.0000 |         - |   14.3 MB |
    | LuceneSimple    | 5           |  16.953 ms |   6.1768 ms |  0.3386 ms |               5.0000 |                - | 1250.0000 | 1000.0000 |   93.7500 |  15.06 MB |
    | ExamineStandard | 15          | 657.503 ms | 195.5415 ms | 10.7183 ms |              15.0000 |                - | 3000.0000 | 1000.0000 |         - |  42.92 MB |
    | LuceneSimple    | 15          |  60.278 ms | 100.6474 ms |  5.5168 ms |              15.0000 |                - | 4333.3333 | 2666.6667 | 1000.0000 |   45.2 MB |

    Without NRT (no diff really)

    | Method          | ThreadCount | Mean      | Error      | StdDev    | Gen0      | Completed Work Items | Lock Contentions | Gen1      | Allocated |
    |---------------- |------------ |----------:|-----------:|----------:|----------:|---------------------:|-----------------:|----------:|----------:|
    | ExamineStandard | 1           |  12.48 ms |   3.218 ms |  0.176 ms |  250.0000 |               1.0000 |           0.0938 |  156.2500 |   3.13 MB |
    | ExamineStandard | 5           | 149.31 ms |  88.914 ms |  4.874 ms | 1000.0000 |               5.0000 |           4.0000 |  750.0000 |   14.7 MB |
    | ExamineStandard | 15          | 613.14 ms | 897.936 ms | 49.219 ms | 3000.0000 |              15.0000 |          14.0000 | 1000.0000 |  43.67 MB |

    Without querying MaxDoc (Shows we were double/triple querying)

    | Method          | ThreadCount | Mean       | Error     | StdDev    | Gen0     | Completed Work Items | Lock Contentions | Gen1     | Allocated   |
    |---------------- |------------ |-----------:|----------:|----------:|---------:|---------------------:|-----------------:|---------:|------------:|
    | ExamineStandard | 1           |   5.223 ms |  1.452 ms | 0.0796 ms |  78.1250 |               1.0000 |           0.0313 |   7.8125 |      962 KB |
    | ExamineStandard | 5           |  26.772 ms |  9.982 ms | 0.5471 ms | 312.5000 |               5.0000 |           4.0000 | 187.5000 |  3825.35 KB |
    | ExamineStandard | 15          | 101.483 ms | 65.690 ms | 3.6007 ms | 800.0000 |              15.0000 |          14.0000 | 400.0000 | 10989.05 KB |

    Without apply deletes (should be faster, we'll keep it)

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

    With Latest changes (don't re-create SearchContext, cache fields if nothing changes, etc...):

    | Method          | ThreadCount | Mean       | Error       | StdDev     | Completed Work Items | Lock Contentions | Gen0      | Gen1      | Allocated   |
    |---------------- |------------ |-----------:|------------:|-----------:|---------------------:|-----------------:|----------:|----------:|------------:|
    | ExamineStandard | 1           |   5.157 ms |   1.0374 ms |  0.0569 ms |               1.0000 |           0.0156 |   78.1250 |   39.0625 |    963.3 KB |
    | LuceneSimple    | 1           |  11.338 ms |   0.8416 ms |  0.0461 ms |               1.0000 |           0.0156 |  265.6250 |  187.5000 |  3269.09 KB |
    | ExamineStandard | 5           |  27.038 ms |   7.2847 ms |  0.3993 ms |               5.0000 |           4.0000 |  312.5000 |  187.5000 |   3812.7 KB |
    | LuceneSimple    | 5           | 144.196 ms | 185.2203 ms | 10.1526 ms |               5.0000 |                - | 1000.0000 |  750.0000 | 15047.06 KB |
    | ExamineStandard | 15          |  95.799 ms |  64.1371 ms |  3.5156 ms |              15.0000 |          14.0000 |  833.3333 |  500.0000 | 10940.31 KB |
    | LuceneSimple    | 15          | 566.652 ms | 275.2278 ms | 15.0862 ms |              15.0000 |                - | 3000.0000 | 1000.0000 |  44485.6 KB |

    Determining the best NRT values

    | Method          | ThreadCount | NrtTargetMaxStaleSec | NrtTargetMinStaleSec | Mean       | Error       | StdDev    | Gen0     | Completed Work Items | Lock Contentions | Gen1     | Allocated   |
    |---------------- |------------ |--------------------- |--------------------- |-----------:|------------:|----------:|---------:|---------------------:|-----------------:|---------:|------------:|
    | ExamineStandard | 1           | 5                    | 1                    |   5.507 ms |   1.7993 ms | 0.0986 ms |  78.1250 |               1.0000 |                - |  31.2500 |   963.59 KB |
    | ExamineStandard | 1           | 5                    | 5                    |   5.190 ms |   0.4792 ms | 0.0263 ms |  78.1250 |               1.0000 |           0.0078 |  39.0625 |   963.65 KB |
    | ExamineStandard | 1           | 60                   | 1                    |   5.406 ms |   2.2636 ms | 0.1241 ms |  78.1250 |               1.0000 |           0.0313 |  31.2500 |   963.71 KB |
    | ExamineStandard | 1           | 60                   | 5                    |   5.316 ms |   3.4301 ms | 0.1880 ms |  78.1250 |               1.0000 |                - |  39.0625 |   963.42 KB |
    | ExamineStandard | 5           | 5                    | 1                    |  26.439 ms |   1.2601 ms | 0.0691 ms | 312.5000 |               5.0000 |           4.0000 | 187.5000 |  3813.45 KB |
    | ExamineStandard | 5           | 5                    | 5                    |  27.341 ms |  13.3950 ms | 0.7342 ms | 312.5000 |               5.0000 |           4.0313 | 187.5000 |  3813.83 KB |
    | ExamineStandard | 5           | 60                   | 1                    |  26.768 ms |   9.4732 ms | 0.5193 ms | 312.5000 |               5.0000 |           4.0000 | 156.2500 |  3814.06 KB |
    | ExamineStandard | 5           | 60                   | 5                    |  27.216 ms |   3.3213 ms | 0.1821 ms | 312.5000 |               5.0000 |           4.0000 | 187.5000 |  3813.83 KB |
    | ExamineStandard | 15          | 5                    | 1                    | 101.040 ms |  44.3254 ms | 2.4296 ms | 800.0000 |              15.0000 |          14.0000 | 600.0000 | 10940.73 KB |
    | ExamineStandard | 15          | 5                    | 5                    | 104.027 ms |  44.7547 ms | 2.4532 ms | 800.0000 |              15.0000 |          14.0000 | 400.0000 | 10939.87 KB |
    | ExamineStandard | 15          | 60                   | 1                    |  96.622 ms | 162.1682 ms | 8.8890 ms | 800.0000 |              15.0000 |          14.0000 | 400.0000 | 10941.64 KB |
    | ExamineStandard | 15          | 60                   | 5                    | 102.469 ms |  78.0316 ms | 4.2772 ms | 800.0000 |              15.0000 |          14.0000 | 400.0000 | 10936.86 KB |

    Putting MaxDoc back in makes it go crazy

    | Method          | ThreadCount | Mean      | Error      | StdDev    | Gen0      | Completed Work Items | Lock Contentions | Gen1      | Allocated |
    |---------------- |------------ |----------:|-----------:|----------:|----------:|---------------------:|-----------------:|----------:|----------:|
    | ExamineStandard | 1           |  12.90 ms |   4.049 ms |  0.222 ms |  250.0000 |               1.0000 |                - |  156.2500 |   3.13 MB |
    | ExamineStandard | 5           | 149.16 ms |  74.884 ms |  4.105 ms | 1000.0000 |               5.0000 |           4.0000 |  750.0000 |  14.69 MB |
    | ExamineStandard | 15          | 635.77 ms | 899.620 ms | 49.311 ms | 3000.0000 |              15.0000 |          14.0000 | 1000.0000 |  43.57 MB |

    Using different MaxResults leads to crazy results

    | Method          | ThreadCount | MaxResults | Mean         | Error         | StdDev      | Completed Work Items | Lock Contentions | Gen0      | Gen1      | Gen2      | Allocated |
    |---------------- |------------ |----------- |-------------:|--------------:|------------:|---------------------:|-----------------:|----------:|----------:|----------:|----------:|
    | ExamineStandard | 15          | 10         |     4.979 ms |     1.6928 ms |   0.0928 ms |              15.0000 |          14.0000 |  257.8125 |  109.3750 |         - |      3 MB |
    | LuceneSimple    | 15          | 10         |     4.168 ms |     0.6606 ms |   0.0362 ms |              15.0000 |           0.0234 |  218.7500 |   93.7500 |         - |   2.57 MB |
    | ExamineStandard | 15          | 100        |    92.838 ms |    88.3517 ms |   4.8429 ms |              15.0000 |          14.0000 |  833.3333 |  666.6667 |         - |  10.68 MB |
    | LuceneSimple    | 15          | 100        |   103.927 ms |    64.1171 ms |   3.5145 ms |              15.0000 |                - |  800.0000 |  600.0000 |         - |  10.33 MB |
    | ExamineStandard | 15          | 1000       | 1,278.769 ms |   826.1505 ms |  45.2841 ms |              15.0000 |          14.0000 | 7000.0000 | 4000.0000 | 1000.0000 |  84.55 MB |
    | LuceneSimple    | 15          | 1000       | 1,248.199 ms | 1,921.5844 ms | 105.3285 ms |              15.0000 |                - | 7000.0000 | 4000.0000 | 1000.0000 |  84.08 MB |

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

        [Params(15)]
        public int ThreadCount { get; set; }

        [Params(10, 100, 1000)]
        public int MaxResults { get; set; }

        [Benchmark]
        public async Task ExamineStandard()
        {
            using var indexer = GetTestIndex(
                _luceneDir,
                _analyzer,
                nrtEnabled: false);

            var tasks = new List<Task>();

            for (var i = 0; i < ThreadCount; i++)
            {
                tasks.Add(new Task(() =>
                {
                    // always resolve the searcher from the indexer
                    var searcher = indexer.Searcher;

                    var query = searcher.CreateQuery("content").Field("nodeName", "location".MultipleCharacterWildcard());
                    var results = query.Execute(QueryOptions.SkipTake(0, MaxResults));

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

                    // this is like doing Acquire, does it perform the same (it will allocate more)
                    using var context = searcherManager.GetContext();

                    var searcher = context.Reference;

                    // Don't use this, increasing the max docs substantially decreases performance
                    //var maxDoc = searcher.IndexReader.MaxDoc;
                    var topDocsCollector = TopScoreDocCollector.Create(MaxResults, null, true);

                    searcher.Search(query, topDocsCollector);
                    var topDocs = topDocsCollector.GetTopDocs(0, MaxResults);

                    var totalItemCount = topDocs.TotalHits;

                    var results = new List<ISearchResult>(topDocs.ScoreDocs.Length);
                    for (var i = 0; i < topDocs.ScoreDocs.Length; i++)
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
                    var maxScore = topDocs.MaxScore;

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
