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
    /// <remarks>
    /// Results (BenchmarkDotNet v0.14.0, .NET 8.0.28, AMD EPYC 9V74 2 physical cores,
    /// ShortRun: 3 warmup / 3 iterations / 1 launch). "Source" is the current support/3.x
    /// build; 3.0.1–3.3.0 are the published NuGet packages.
    ///
    /// | Method                | Job    | Mean        | Allocated | Alloc Ratio |
    /// |---------------------- |------- |------------:|----------:|------------:|
    /// | CreateQueryOnly       | 3.3.0  |  3,995.1 ns |   8.34 KB |        1.00 |
    /// | GroupedAndStringArray | 3.3.0  | 21,376.9 ns |  21.10 KB |        2.53 |
    /// | GroupedOrStringArray  | 3.3.0  | 22,342.3 ns |  21.10 KB |        2.53 |
    /// | GroupedNotStringArray | 3.3.0  | 22,001.0 ns |  21.34 KB |        2.56 |
    /// | CreateQueryOnly       | Source |    318.6 ns |   2.20 KB |        1.00 |
    /// | GroupedAndStringArray | Source | 16,659.1 ns |  14.34 KB |        6.53 |
    /// | GroupedOrStringArray  | Source | 17,214.5 ns |  14.34 KB |        6.53 |
    /// | GroupedNotStringArray | Source | 16,807.1 ns |  14.58 KB |        6.64 |
    ///
    /// The earlier 3.0.1/3.1.0/3.2.1 packages match 3.3.0 within noise (~21.1 KB grouped,
    /// 8.34 KB CreateQueryOnly). The source build cuts the grouped-clause allocation from
    /// ~21 KB to ~14.3 KB (the string[] fast-path avoids the defensive .ToArray() copy) and
    /// the CreateQuery() baseline from 8.34 KB to 2.2 KB, while also running ~25% faster.
    /// </remarks>
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
            _analyzer.Dispose();
            base.TearDown();
            if (_tempBasePath != null)
            {
                System.IO.Directory.Delete(_tempBasePath, true);
            }
        }

        /// <summary>
        /// Baseline cost for query creation without any grouped clause operations.
        /// </summary>
        [Benchmark(Baseline = true)]
        public IQuery CreateQueryOnly()
            => _searcher!.CreateQuery();

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
