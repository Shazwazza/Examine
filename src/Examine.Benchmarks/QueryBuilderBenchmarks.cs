using BenchmarkDotNet.Attributes;
using Examine.Lucene.Providers;
using Examine.Search;
using Lucene.Net.Analysis.Standard;
using Microsoft.Extensions.Logging;

namespace Examine.Benchmarks
{
    /// <summary>
    /// Measures allocation cost of building queries with GroupedAnd/Or/Not.
    /// Highlights the string[] fast-path: when the caller already passes a string[],
    /// the source build skips the defensive .ToArray() copy entirely.
    /// </summary>
    [Config(typeof(NugetConfig))]
    [HideColumns("Arguments", "StdDev", "Error", "NuGetReferences")]
    [MemoryDiagnoser]
    public class QueryBuilderBenchmarks : ExamineBaseTest
    {
        private readonly StandardAnalyzer _analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
        private ILogger<QueryBuilderBenchmarks>? _logger;
        private string? _tempBasePath;
        private LuceneIndex? _indexer;
        private ISearcher? _searcher;

        // Pre-created string[] — common caller pattern; the optimized path avoids copying it
        private static readonly string[] s_fields = ["title", "body", "description", "tags", "author"];

        [GlobalSetup]
        public override void Setup()
        {
            base.Setup();

            _logger = LoggerFactory!.CreateLogger<QueryBuilderBenchmarks>();
            _tempBasePath = Path.Combine(Path.GetTempPath(), "ExamineBenchmarks");
            _indexer = InitTools.InitializeIndex(this, _tempBasePath, _analyzer, out _);
            _indexer.IndexItems(new[]
            {
                ValueSet.FromObject("1", "content", new { title = "test", body = "sample", description = "desc" })
            });
            _searcher = _indexer.Searcher;

            _logger.LogInformation("Query builder benchmark index ready");
        }

        [GlobalCleanup]
        public override void TearDown()
        {
            _indexer?.Dispose();
            base.TearDown();
            if (_tempBasePath != null)
            {
                System.IO.Directory.Delete(_tempBasePath, true);
            }
        }

        /// <summary>
        /// GroupedAnd with a pre-existing string[] for fields — the optimized path
        /// avoids the defensive .ToArray() copy.
        /// </summary>
        [Benchmark]
        public IBooleanOperation GroupedAndStringArray()
            => _searcher!.CreateQuery().GroupedAnd(s_fields, "value1", "value2");

        /// <summary>
        /// GroupedOr with a pre-existing string[] for fields.
        /// </summary>
        [Benchmark]
        public IBooleanOperation GroupedOrStringArray()
            => _searcher!.CreateQuery().GroupedOr(s_fields, "value1", "value2");

        /// <summary>
        /// GroupedNot with a pre-existing string[] for fields.
        /// </summary>
        [Benchmark]
        public IBooleanOperation GroupedNotStringArray()
            => _searcher!.CreateQuery().GroupedNot(s_fields, "value1", "value2");

#if RELEASE
        protected override ILoggerFactory CreateLoggerFactory()
            => Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
#endif
    }
}
