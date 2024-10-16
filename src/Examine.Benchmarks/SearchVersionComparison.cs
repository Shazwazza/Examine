using BenchmarkDotNet.Attributes;
using Examine.Lucene.Providers;
using Lucene.Net.Analysis.Standard;
using Microsoft.Extensions.Logging;

namespace Examine.Benchmarks
{
    [Config(typeof(NugetConfig))]
    [HideColumns("Arguments", "StdDev", "Error", "NuGetReferences")]
    [MemoryDiagnoser]
    public class SearchVersionComparison : ExamineBaseTest
    {
        private readonly List<ValueSet> _valueSets = InitTools.CreateValueSet(10000);
        private readonly StandardAnalyzer _analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
        private ILogger<SearchVersionComparison>? _logger;
        private string? _tempBasePath;
        private LuceneIndex? _indexer;

        [GlobalSetup]
        public override void Setup()
        {
            base.Setup();

            _logger = LoggerFactory.CreateLogger<SearchVersionComparison>();
            _tempBasePath = Path.Combine(Path.GetTempPath(), "ExamineTests");
            _indexer = InitTools.InitializeIndex(this, _tempBasePath, _analyzer, out _);
            _indexer!.IndexItems(_valueSets);

            _logger.LogInformation("Indexed {DocumentCount} documents", _valueSets.Count);
        }

        [GlobalCleanup]
        public override void TearDown()
        {
            _indexer!.Dispose();
            base.TearDown();
            Directory.Delete(_tempBasePath!, true);
        }

        [Params(1, 25, 100)]
        public int ThreadCount { get; set; }

        [Benchmark]
        public async Task ConcurrentSearch()
        {
            var tasks = new List<Task>();

            for (var i = 0; i < ThreadCount; i++)
            {
                tasks.Add(new Task(() =>
                {
                    // always resolve the searcher from the indexer
                    var searcher = _indexer!.Searcher;

                    var query = searcher.CreateQuery().Field("nodeName", "location1");
                    var results = query.Execute();

                    // enumerate (forces the result to execute)
                    var logOutput = "ThreadID: " + Thread.CurrentThread.ManagedThreadId + ", Results: " + string.Join(',', results.Select(x => $"{x.Id}-{x.Values.Count}-{x.Score}").ToArray());
                    _logger!.LogDebug(logOutput);

                    //_logger!.LogInformation("Results: {Results}", results.TotalItemCount);
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
    }
}
