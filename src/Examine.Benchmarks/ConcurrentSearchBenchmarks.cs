using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;

//[assembly: Config(typeof(MyDefaultConfig))]

//internal class MyDefaultConfig : ManualConfig
//{
//    public MyDefaultConfig()
//    {
//        WithOptions(ConfigOptions.DisableOptimizationsValidator);
//    }
//}

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
    UPDATE: We cannot, that is specialized and we cannot support it.

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

    After changing to use singleton indexers/managers

    | Method          | ThreadCount | MaxResults | Mean           | Error         | StdDev       | Completed Work Items | Lock Contentions | Gen0       | Gen1       | Gen2      | Allocated    |
    |---------------- |------------ |----------- |---------------:|--------------:|-------------:|---------------------:|-----------------:|-----------:|-----------:|----------:|-------------:|
    | ExamineStandard | 1           | 10         |       101.9 μs |       9.70 μs |      0.53 μs |               1.0000 |           0.0029 |    12.6953 |     0.9766 |         - |    157.77 KB |
    | LuceneSimple    | 1           | 10         |       120.7 us |       9.33 us |      0.51 us |               1.0000 |           0.0022 |    11.4746 |     1.2207 |         - |    141.66 KB |
    | ExamineStandard | 1           | 100        |     1,555.0 us |     407.07 us |     22.31 us |               1.0000 |           0.0078 |    54.6875 |    15.6250 |         - |    681.92 KB |
    | LuceneSimple    | 1           | 100        |     1,598.8 μs |     233.79 μs |     12.81 μs |               1.0000 |           0.0078 |    52.7344 |    17.5781 |         - |    664.64 KB |
    | ExamineStandard | 1           | 1000       |    17,449.3 μs |   1,472.32 μs |     80.70 μs |               1.0000 |                - |   437.5000 |   312.5000 |   31.2500 |   5723.12 KB |
    | LuceneSimple    | 1           | 1000       |    17,739.7 μs |   3,797.03 μs |    208.13 μs |               1.0000 |           0.0313 |   437.5000 |   312.5000 |   31.2500 |   5698.42 KB |
    | ExamineStandard | 15          | 10         |     1,630.6 μs |   2,436.46 μs |    133.55 μs |              15.0000 |           0.0430 |   195.3125 |    15.6250 |         - |   2362.51 KB |
    | LuceneSimple    | 15          | 10         |     1,742.6 μs |     214.81 μs |     11.77 μs |              15.0000 |           0.0820 |   179.6875 |    27.3438 |         - |   2118.47 KB |
    | ExamineStandard | 15          | 100        |   105,817.2 μs |  28,398.55 μs |  1,556.62 μs |              15.0000 |                - |   833.3333 |   666.6667 |         - |  10225.39 KB |
    | LuceneSimple    | 15          | 100        |    95,732.1 μs |  57,903.39 μs |  3,173.88 μs |              15.0000 |                - |   666.6667 |   500.0000 |         - |    9967.2 KB |
    | ExamineStandard | 15          | 1000       | 1,125,955.0 μs | 822,782.38 μs | 45,099.48 μs |              15.0000 |                - |  7000.0000 |  4000.0000 | 1000.0000 |   85877.8 KB |
    | LuceneSimple    | 15          | 1000       | 1,446,507.5 μs | 855,107.53 μs | 46,871.33 μs |              15.0000 |                - |  7000.0000 |  4000.0000 | 1000.0000 |  85509.77 KB |
    | ExamineStandard | 30          | 10         |     4,261.3 μs |   1,676.61 μs |     91.90 μs |              30.0000 |           0.3047 |   390.6250 |    70.3125 |         - |   4724.59 KB |
    | LuceneSimple    | 30          | 10         |     3,895.8 μs |   1,768.88 μs |     96.96 μs |              30.0000 |           0.1250 |   359.3750 |    46.8750 |         - |   4237.24 KB |
    | ExamineStandard | 30          | 100        |   232,909.0 μs |  30,215.14 μs |  1,656.19 μs |              30.0000 |                - |  1500.0000 |  1000.0000 |         - |  20455.26 KB |
    | LuceneSimple    | 30          | 100        |   259,557.3 μs |  40,643.51 μs |  2,227.81 μs |              30.0000 |                - |  1500.0000 |  1000.0000 |         - |  19940.39 KB |
    | ExamineStandard | 30          | 1000       | 2,886,589.2 μs | 328,362.02 μs | 17,998.63 μs |              30.0000 |           1.0000 | 16000.0000 | 11000.0000 | 3000.0000 | 171858.03 KB |
    | LuceneSimple    | 30          | 1000       | 2,662,715.9 μs | 898,686.63 μs | 49,260.05 μs |              30.0000 |                - | 16000.0000 | 11000.0000 | 3000.0000 | 171094.02 KB |


    */
    [LongRunJob(RuntimeMoniker.Net80)]
    [ThreadingDiagnoser]
    [MemoryDiagnoser]
    //[DotNetCountersDiagnoser]
    //[CPUUsageDiagnoser]
    public class ConcurrentSearchBenchmarks : ExamineBaseTest
    {
        private readonly StandardAnalyzer _analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
        private ILogger<ConcurrentSearchBenchmarks>? _logger;
        private string? _tempBasePath;
        private TestIndex? _indexer;
        private FSDirectory? _indexDir;
        private IndexWriter? _writer;
        private SearcherManager? _searcherManager;

        [GlobalSetup]
        public override void Setup()
        {
            base.Setup();

            _logger = LoggerFactory.CreateLogger<ConcurrentSearchBenchmarks>();
            _tempBasePath = Path.Combine(Path.GetTempPath(), "ExamineTests");

            // indexer for examine
            _indexer = InitializeAndIndexItems(_tempBasePath, _analyzer, out _);

            // indexer for lucene
            var tempIndexer = InitializeAndIndexItems(_tempBasePath, _analyzer, out var indexDir);
            tempIndexer.Dispose();
            _indexDir = FSDirectory.Open(indexDir);
            var writerConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, _analyzer);
            //writerConfig.SetMaxBufferedDocs(1000);
            //writerConfig.SetReaderTermsIndexDivisor(4);
            //writerConfig.SetOpenMode(OpenMode.APPEND);
            //writerConfig.SetReaderPooling(true);
            //writerConfig.SetCodec(new Lucene46Codec());
            _writer = new IndexWriter(_indexDir, writerConfig);
            var trackingWriter = new TrackingIndexWriter(_writer);
            _searcherManager = new SearcherManager(trackingWriter.IndexWriter, applyAllDeletes: true, new SearcherFactory());
        }

        [GlobalCleanup]
        public override void TearDown()
        {
            _indexer.Dispose();
            _searcherManager.Dispose();
            _writer.Dispose();
            _indexDir.Dispose();

            base.TearDown();

            System.IO.Directory.Delete(_tempBasePath, true);
        }

        [Params(1, 50, 100)]
        public int ThreadCount { get; set; }

        [Params(10/*, 100, 1000*/)]
        public int MaxResults { get; set; }

        [Benchmark(Baseline = true)]
        public async Task ExamineStandard()
        {
            var tasks = new List<Task>();

            for (var i = 0; i < ThreadCount; i++)
            {
                tasks.Add(new Task(() =>
                {
                    // always resolve the searcher from the indexer
                    var searcher = _indexer.Searcher;

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
        public async Task SimpleMultiThreadLoop()
        {
            var tasks = new List<Task>();

            for (var i = 0; i < ThreadCount; i++)
            {
                tasks.Add(new Task(() =>
                {
                }));
            }

            foreach (var task in tasks)
            {
                task.Start();
            }

            await Task.WhenAll(tasks);
        }

        public async Task TestAcquireThreadContention()
        {
            var tasks = new List<Task>();

            for (var i = 0; i < ThreadCount; i++)
            {
                tasks.Add(new Task(() =>
                {
                    var parser = new QueryParser(LuceneVersion.LUCENE_48, ExamineFieldNames.ItemIdFieldName, new StandardAnalyzer(LuceneVersion.LUCENE_48));
                    var query = parser.Parse($"{ExamineFieldNames.CategoryFieldName}:content AND nodeName:location*");

                    // this is like doing Acquire, does it perform the same (it will allocate more)
                    using var context = _searcherManager.GetContext();

                    var searcher = context.Reference;

                    // Don't use this, increasing the max docs substantially decreases performance
                    //var maxDoc = searcher.IndexReader.MaxDoc;
                    var topDocsCollector = TopScoreDocCollector.Create(MaxResults, null, true);

                    searcher.Search(query, topDocsCollector);

                    var topDocs = topDocsCollector.GetTopDocs(0, MaxResults);

                    var totalItemCount = topDocs.TotalHits;
                    var maxScore = topDocs.MaxScore;
                }));
            }

            foreach (var task in tasks)
            {
                task.Start();
            }

            await Task.WhenAll(tasks);
        }

        [Benchmark]
        public async Task LuceneAcquireAlways()
        {
            var tasks = new List<Task>();

            for (var i = 0; i < ThreadCount; i++)
            {
                tasks.Add(new Task(() =>
                {
                    var parser = new QueryParser(LuceneVersion.LUCENE_48, ExamineFieldNames.ItemIdFieldName, new StandardAnalyzer(LuceneVersion.LUCENE_48));
                    var query = parser.Parse($"{ExamineFieldNames.CategoryFieldName}:content AND nodeName:location*");

                    // this is like doing Acquire, does it perform the same (it will allocate more)
                    using var context = _searcherManager.GetContext();

                    var searcher = context.Reference;

                    // Don't use this, increasing the max docs substantially decreases performance
                    //var maxDoc = searcher.IndexReader.MaxDoc;
                    var topDocsCollector = TopScoreDocCollector.Create(MaxResults, null, true);

                    searcher.Search(query, topDocsCollector);

                    var topDocs = topDocsCollector.GetTopDocs(0, MaxResults);

                    var totalItemCount = topDocs.TotalHits;

                    var results = new List<LuceneSearchResult>(topDocs.ScoreDocs.Length);

                    foreach (var scoreDoc in topDocs.ScoreDocs)
                    {
                        var docId = scoreDoc.Doc;
                        var score = scoreDoc.Score;
                        var shardIndex = scoreDoc.ShardIndex;
                        var doc = searcher.Doc(docId);
                        var result = LuceneSearchExecutor.CreateSearchResult(doc, score, shardIndex);
                        results.Add(result);
                    }

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

        [Benchmark]
        public async Task LuceneAcquireAlwaysWithLock()
        {
            var tasks = new List<Task>();
            var myLock = new object();

            for (var i = 0; i < ThreadCount; i++)
            {
                tasks.Add(new Task(() =>
                {
                    lock (myLock)
                    {
                        var parser = new QueryParser(LuceneVersion.LUCENE_48, ExamineFieldNames.ItemIdFieldName, new StandardAnalyzer(LuceneVersion.LUCENE_48));
                        var query = parser.Parse($"{ExamineFieldNames.CategoryFieldName}:content AND nodeName:location*");

                        // this is like doing Acquire, does it perform the same (it will allocate more)
                        using var context = _searcherManager.GetContext();

                        var searcher = context.Reference;

                        // Don't use this, increasing the max docs substantially decreases performance
                        //var maxDoc = searcher.IndexReader.MaxDoc;
                        var topDocsCollector = TopScoreDocCollector.Create(MaxResults, null, true);

                        searcher.Search(query, topDocsCollector);

                        var topDocs = topDocsCollector.GetTopDocs(0, MaxResults);

                        var totalItemCount = topDocs.TotalHits;

                        var results = new List<LuceneSearchResult>(topDocs.ScoreDocs.Length);

                        foreach (var scoreDoc in topDocs.ScoreDocs)
                        {
                            var docId = scoreDoc.Doc;
                            var score = scoreDoc.Score;
                            var shardIndex = scoreDoc.ShardIndex;
                            var doc = searcher.Doc(docId);
                            var result = LuceneSearchExecutor.CreateSearchResult(doc, score, shardIndex);
                            results.Add(result);
                        }

                        var maxScore = topDocs.MaxScore;

                        // enumerate (forces the result to execute)
                        var logOutput = "ThreadID: " + Thread.CurrentThread.ManagedThreadId + ", Results: " + string.Join(',', results.Select(x => $"{x.Id}-{x.Values.Count}-{x.Score}").ToArray());
                        _logger.LogDebug(logOutput);
                    }
                }));
            }

            foreach (var task in tasks)
            {
                task.Start();
            }

            await Task.WhenAll(tasks);
        }

        [Benchmark]
        public async Task LuceneAcquireOnce()
        {
            var tasks = new List<Task>();

            var searcher = _searcherManager.Acquire();

            try
            {
                for (var i = 0; i < ThreadCount; i++)
                {
                    tasks.Add(new Task(() =>
                    {
                        var parser = new QueryParser(LuceneVersion.LUCENE_48, ExamineFieldNames.ItemIdFieldName, new StandardAnalyzer(LuceneVersion.LUCENE_48));
                        var query = parser.Parse($"{ExamineFieldNames.CategoryFieldName}:content AND nodeName:location*");

                        // Don't use this, increasing the max docs substantially decreases performance
                        //var maxDoc = searcher.IndexReader.MaxDoc;
                        var topDocsCollector = TopScoreDocCollector.Create(MaxResults, null, true);

                        searcher.Search(query, topDocsCollector);
                        var topDocs = topDocsCollector.GetTopDocs(0, MaxResults);

                        var totalItemCount = topDocs.TotalHits;

                        var results = new List<LuceneSearchResult>(topDocs.ScoreDocs.Length);
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
            finally
            {
                _searcherManager.Release(searcher);
            }
        }

        [Benchmark]
        public async Task LuceneSortedDocIds()
        {
            var tasks = new List<Task>();

            for (var i = 0; i < ThreadCount; i++)
            {
                tasks.Add(new Task(() =>
                {
                    var parser = new QueryParser(LuceneVersion.LUCENE_48, ExamineFieldNames.ItemIdFieldName, new StandardAnalyzer(LuceneVersion.LUCENE_48));
                    var query = parser.Parse($"{ExamineFieldNames.CategoryFieldName}:content AND nodeName:location*");

                    // this is like doing Acquire, does it perform the same (it will allocate more)
                    using var context = _searcherManager.GetContext();

                    var searcher = context.Reference;

                    // Don't use this, increasing the max docs substantially decreases performance
                    //var maxDoc = searcher.IndexReader.MaxDoc;
                    var topDocsCollector = TopScoreDocCollector.Create(MaxResults, null, true);

                    searcher.Search(query, topDocsCollector);

                    var topDocs = topDocsCollector.GetTopDocs(0, MaxResults);

                    var totalItemCount = topDocs.TotalHits;

                    var results = new List<LuceneSearchResult>(topDocs.ScoreDocs.Length);

                    foreach (var scoreDoc in topDocs.ScoreDocs.OrderBy(x => x.Doc))
                    {
                        var docId = scoreDoc.Doc;
                        var score = scoreDoc.Score;
                        var shardIndex = scoreDoc.ShardIndex;
                        var doc = searcher.Doc(docId);
                        var result = LuceneSearchExecutor.CreateSearchResult(doc, score, shardIndex);
                        results.Add(result);
                    }

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

#if RELEASE
        protected override ILoggerFactory CreateLoggerFactory()
            => Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
#endif
        private TestIndex InitializeAndIndexItems(
            string tempBasePath,
            Analyzer analyzer,
            out DirectoryInfo indexDir)
        {
            var tempPath = Path.Combine(tempBasePath, Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(tempPath);
            indexDir = new DirectoryInfo(tempPath);
            var luceneDirectory = FSDirectory.Open(indexDir);
            var indexer = GetTestIndex(luceneDirectory, analyzer);

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

            return indexer;
        }
    }
}
