using BenchmarkDotNet.Attributes;
using Examine.Lucene.Providers;
using Examine.Search;
using Lucene.Net.Analysis.Standard;
using Microsoft.Extensions.Logging;

namespace Examine.Benchmarks
{
    /// <summary>
    /// Measures the allocation and execution cost of <see cref="IQuery.ManagedQuery"/> — the
    /// primary full-text search API used by <see cref="BaseLuceneSearcher.Search(string, QueryOptions)"/>.
    ///
    /// Key hot paths exercised:
    /// - <c>ManagedQueryInternal</c>: builds a <c>LateBoundQuery</c> that lazily calls <c>GetFieldValueType</c>
    ///   per field on first <c>Execute()</c>.
    /// - <c>SearchContext.GetFieldValueType</c>: resolved via the <c>_defaultFactory</c> volatile cache
    ///   (introduced in PR #520) after the first call.
    /// - <c>LuceneSearchExecutor.CheckQueryForExtractTerms</c>: validates the <c>LateBoundQuery</c> tree
    ///   before extracting terms.
    ///
    /// Varying the field count shows per-field savings from the factory cache.
    /// </summary>
    /// <remarks>
    /// Results (AMD EPYC 7763, .NET 8.0.28, ShortRun — 3 warmup + 3 iterations, 1000-document index):
    ///
    /// | Method                  | Job    | Mean      | Ratio | Allocated  | Alloc Ratio |
    /// |------------------------ |------- |----------:|------:|-----------:|------------:|
    /// | ManagedQueryAllFields   | 3.0.1  | 11.612 ms |  1.00 | 1327.37 KB |        1.00 |
    /// | ManagedQuerySingleField | 3.0.1  | 12.082 ms |  1.04 | 1261.15 KB |        0.95 |
    /// | ManagedQueryTwoFields   | 3.0.1  | 11.658 ms |  1.00 |  1283.4 KB |        0.97 |
    /// | ManagedQueryThreeFields | 3.0.1  | 11.695 ms |  1.01 | 1306.22 KB |        0.98 |
    /// |                         |        |           |       |            |             |
    /// | ManagedQueryAllFields   | 3.1.0  | 11.730 ms |  1.00 | 1326.76 KB |        1.00 |
    /// | ManagedQuerySingleField | 3.1.0  | 11.885 ms |  1.01 | 1261.15 KB |        0.95 |
    /// | ManagedQueryTwoFields   | 3.1.0  | 11.776 ms |  1.00 | 1283.45 KB |        0.97 |
    /// | ManagedQueryThreeFields | 3.1.0  | 11.793 ms |  1.01 | 1306.24 KB |        0.98 |
    /// |                         |        |           |       |            |             |
    /// | ManagedQueryAllFields   | 3.2.1  | 11.527 ms |  1.00 | 1323.17 KB |        1.00 |
    /// | ManagedQuerySingleField | 3.2.1  | 12.769 ms |  1.11 | 1257.01 KB |        0.95 |
    /// | ManagedQueryTwoFields   | 3.2.1  | 11.359 ms |  0.99 | 1279.31 KB |        0.97 |
    /// | ManagedQueryThreeFields | 3.2.1  | 11.779 ms |  1.02 | 1302.16 KB |        0.98 |
    /// |                         |        |           |       |            |             |
    /// | ManagedQueryAllFields   | 3.3.0  | 11.421 ms |  1.00 |  1323.2 KB |        1.00 |
    /// | ManagedQuerySingleField | 3.3.0  | 12.165 ms |  1.07 | 1257.01 KB |        0.95 |
    /// | ManagedQueryTwoFields   | 3.3.0  | 11.604 ms |  1.02 | 1279.27 KB |        0.97 |
    /// | ManagedQueryThreeFields | 3.3.0  | 13.193 ms |  1.16 | 1302.13 KB |        0.98 |
    /// |                         |        |           |       |            |             |
    /// | ManagedQueryAllFields   | Source |  2.171 ms |  1.00 |  371.29 KB |        1.00 |
    /// | ManagedQuerySingleField | Source |  2.181 ms |  1.00 |  306.49 KB |        0.83 |
    /// | ManagedQueryTwoFields   | Source |  2.286 ms |  1.05 |  328.38 KB |        0.88 |
    /// | ManagedQueryThreeFields | Source |  2.210 ms |  1.02 |  351.01 KB |        0.95 |
    ///
    /// Source vs 3.3.0 (most recent NuGet release):
    /// - Speed: ~5.3x faster (2.2 ms vs 11.4–13.2 ms)
    /// - Allocations: ~3.6x less (351–371 KB vs 1,279–1,327 KB)
    ///
    /// The gains reflect the cumulative effect of:
    /// - PR #512: eliminated redundant ConcurrentDictionary lookups in AddDocument (warms the _resolvedValueTypes cache)
    /// - PR #516: early return in CheckQueryForExtractTerms after BooleanQuery (saves type-checks + reflection per search)
    /// - PR #520: _defaultFactory volatile cache in SearchContext.GetFieldValueType (eliminates 1 ConcurrentDictionary.TryGetValue per field per query after warm-up)
    /// </remarks>
    [Config(typeof(NugetConfig))]
    [HideColumns("Arguments", "StdDev", "Error", "NuGetReferences")]
    [MemoryDiagnoser]
    public class ManagedQueryBenchmarks : ExamineBaseTest
    {
        private readonly StandardAnalyzer _analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
        private ILogger<ManagedQueryBenchmarks>? _logger;
        private string? _tempBasePath;
        private LuceneIndex? _indexer;
        private ISearcher? _searcher;

        // Fields present in the benchmark index (see InitTools.CreateValueSet):
        // nodeName, bodyText, number, date — all indexed as FullText by default.
        private static readonly string[] s_twoFields = ["nodeName", "bodyText"];
        private static readonly string[] s_threeFields = ["nodeName", "bodyText", "date"];

        [GlobalSetup]
        public override void Setup()
        {
            base.Setup();

            _logger = LoggerFactory!.CreateLogger<ManagedQueryBenchmarks>();
            _tempBasePath = Path.Combine(Path.GetTempPath(), "ExamineBenchmarks");
            _indexer = InitTools.InitializeIndex(this, _tempBasePath, _analyzer, out _);
            _indexer.IndexItems(InitTools.CreateValueSet(1000));
            _searcher = _indexer.Searcher;

            _logger.LogInformation("ManagedQuery benchmark index ready with 1000 docs");
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
        /// Full-text search across all searchable fields — equivalent to calling
        /// <see cref="ISearcher.Search(string, QueryOptions)"/>.
        /// This is the baseline: the most common real-world Examine search entry point.
        /// </summary>
        [Benchmark(Baseline = true)]
        public ISearchResults ManagedQueryAllFields()
            => _searcher!.CreateQuery().ManagedQuery("location1").Execute();

        /// <summary>
        /// Single explicit field — one <c>GetFieldValueType</c> call per execution.
        /// </summary>
        [Benchmark]
        public ISearchResults ManagedQuerySingleField()
            => _searcher!.CreateQuery().ManagedQuery("location1", ["nodeName"]).Execute();

        /// <summary>
        /// Two explicit fields — two <c>GetFieldValueType</c> calls per execution.
        /// Shows per-field factory-cache savings vs older NuGet versions.
        /// </summary>
        [Benchmark]
        public ISearchResults ManagedQueryTwoFields()
            => _searcher!.CreateQuery().ManagedQuery("location1", s_twoFields).Execute();

        /// <summary>
        /// Three explicit fields — three <c>GetFieldValueType</c> calls per execution.
        /// Higher field count amplifies the factory-cache savings per query.
        /// </summary>
        [Benchmark]
        public ISearchResults ManagedQueryThreeFields()
            => _searcher!.CreateQuery().ManagedQuery("location1", s_threeFields).Execute();

#if RELEASE
        protected override ILoggerFactory CreateLoggerFactory()
            => Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
#endif
    }
}
