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
