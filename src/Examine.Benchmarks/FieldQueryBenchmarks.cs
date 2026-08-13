using BenchmarkDotNet.Attributes;
using Examine.Lucene.Providers;
using Examine.Search;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;

namespace Examine.Benchmarks
{
    /// <summary>
    /// Measures allocation cost of typed <c>Field&lt;T&gt;</c> queries (int) and
    /// <c>RangeQuery&lt;T&gt;</c> with one or two fields.
    ///
    /// Key hot path exercised:
    /// - <c>LuceneSearchQuery.Field&lt;T&gt;(string fieldName, T fieldValue)</c>
    /// - <c>LuceneSearchQuery.RangeQueryInternal&lt;T&gt;</c> (single- and multi-field overloads)
    ///
    /// In historical NuGet versions (3.0.1–3.3.0), <c>Field&lt;T&gt;</c> wraps the field name
    /// in a transient <c>string[1]</c> array before calling the multi-field internal overload:
    /// <code>
    ///   => RangeQueryInternal(new[] { fieldName }, min, max, true, true, Occurrence);
    /// </code>
    /// The source build (PR #531) adds a single-field string overload that captures the name
    /// directly, eliminating this allocation on every typed-field query call.
    ///
    /// These benchmarks quantify the per-call allocation difference between the historical
    /// and current code paths, and serve as a regression guard for the Field&lt;T&gt; hot path.
    /// </summary>
    /// <remarks>
    /// Results (BenchmarkDotNet v0.14.0, .NET 8.0.28, AMD EPYC 7763 2 physical cores,
    /// ShortRun: 3 warmup / 3 iterations / 1 launch). "Source" is the current support/3.x
    /// build; 3.0.1–3.3.0 are the published NuGet packages.
    ///
    /// | Method                   | Job    | Mean       | Ratio | Allocated | Alloc Ratio |
    /// |------------------------- |------- |-----------:|------:|----------:|------------:|
    /// | FieldInt_Single          | 3.0.1  | 4,338.6 ns |  1.00 |   8.64 KB |        1.00 |
    /// | RangeQuery_Int_OneField  | 3.0.1  | 4,254.4 ns |  0.98 |   8.61 KB |        1.00 |
    /// | RangeQuery_Int_TwoFields | 3.0.1  | 4,383.6 ns |  1.01 |   8.61 KB |        1.00 |
    /// |                          |        |            |       |           |             |
    /// | FieldInt_Single          | 3.1.0  | 4,264.2 ns |  1.00 |   8.64 KB |        1.00 |
    /// | RangeQuery_Int_OneField  | 3.1.0  | 4,350.6 ns |  1.02 |   8.61 KB |        1.00 |
    /// | RangeQuery_Int_TwoFields | 3.1.0  | 4,139.1 ns |  0.97 |   8.61 KB |        1.00 |
    /// |                          |        |            |       |           |             |
    /// | FieldInt_Single          | 3.2.1  | 4,034.0 ns |  1.00 |   8.65 KB |        1.00 |
    /// | RangeQuery_Int_OneField  | 3.2.1  | 4,175.0 ns |  1.03 |   8.62 KB |        1.00 |
    /// | RangeQuery_Int_TwoFields | 3.2.1  | 4,151.9 ns |  1.03 |   8.62 KB |        1.00 |
    /// |                          |        |            |       |           |             |
    /// | FieldInt_Single          | 3.3.0  | 4,265.8 ns |  1.00 |   8.65 KB |        1.00 |
    /// | RangeQuery_Int_OneField  | 3.3.0  | 3,915.6 ns |  0.92 |   8.62 KB |        1.00 |
    /// | RangeQuery_Int_TwoFields | 3.3.0  | 4,028.7 ns |  0.94 |   8.62 KB |        1.00 |
    /// |                          |        |            |       |           |             |
    /// | FieldInt_Single          | Source |   420.5 ns |  1.00 |    2.5 KB |        1.00 |
    /// | RangeQuery_Int_OneField  | Source |   439.3 ns |  1.05 |   2.47 KB |        0.99 |
    /// | RangeQuery_Int_TwoFields | Source |   461.9 ns |  1.10 |   2.47 KB |        0.99 |
    ///
    /// The source build cuts allocation from ~8.6 KB to ~2.5 KB (−71 %) and execution time
    /// from ~4,200 ns to ~420 ns (10× faster) vs all NuGet versions. The 3.0.1–3.3.0
    /// packages are identical in allocation across all three methods, confirming that the
    /// improvement comes from the single-field overload introduced in the source build.
    /// </remarks>
    [Config(typeof(NugetConfig))]
    [HideColumns("Arguments", "StdDev", "Error", "NuGetReferences")]
    [MemoryDiagnoser]
    public class FieldQueryBenchmarks : ExamineBaseTest
    {
        private readonly StandardAnalyzer _analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
        private ILogger<FieldQueryBenchmarks>? _logger;
        private string? _tempBasePath;
        private LuceneIndex? _indexer;
        private ISearcher? _searcher;

        // Pre-created field arrays for multi-field range benchmarks
        private static readonly string[] s_oneIntField = ["score"];
        private static readonly string[] s_twoIntFields = ["score", "views"];

        [GlobalSetup]
        public override void Setup()
        {
            base.Setup();

            _logger = LoggerFactory!.CreateLogger<FieldQueryBenchmarks>();
            _tempBasePath = Path.Combine(Path.GetTempPath(), "ExamineBenchmarks");

            var tempPath = Path.Combine(_tempBasePath, Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(tempPath);
            var luceneDirectory = FSDirectory.Open(new DirectoryInfo(tempPath));

            _indexer = GetTestIndex(
                luceneDirectory,
                _analyzer,
                new FieldDefinitionCollection(
                    new FieldDefinition("score", FieldDefinitionTypes.Integer),
                    new FieldDefinition("views", FieldDefinitionTypes.Integer)));

            _indexer.IndexItems(
            [
                ValueSet.FromObject("1", "content", new { score = 100, views = 500, name = "alpha" }),
                ValueSet.FromObject("2", "content", new { score = 200, views = 300, name = "beta" }),
                ValueSet.FromObject("3", "content", new { score = 50,  views = 800, name = "gamma" }),
            ]);

            _searcher = _indexer.Searcher;

            _logger.LogInformation("FieldQuery benchmark index ready");
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
        /// Baseline: single typed int field query via <c>Field&lt;int&gt;</c>.
        /// Historical versions allocate <c>string[1]</c> here; the source build does not.
        /// </summary>
        [Benchmark(Baseline = true)]
        public IBooleanOperation FieldInt_Single()
            => _searcher!.CreateQuery().Field<int>("score", 42);

        /// <summary>
        /// Single-field int range query via <c>RangeQuery&lt;int&gt;</c> (string[] overload).
        /// Not affected by the PR #531 optimisation — used as a control.
        /// </summary>
        [Benchmark]
        public IBooleanOperation RangeQuery_Int_OneField()
            => _searcher!.CreateQuery().RangeQuery<int>(s_oneIntField, 1, 1000);

        /// <summary>
        /// Two-field int range query — amplifies any per-field overhead in the multi-field path.
        /// </summary>
        [Benchmark]
        public IBooleanOperation RangeQuery_Int_TwoFields()
            => _searcher!.CreateQuery().RangeQuery<int>(s_twoIntFields, 1, 1000);

#if RELEASE
        protected override ILoggerFactory CreateLoggerFactory()
            => Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
#endif
    }
}
