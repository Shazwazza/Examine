using BenchmarkDotNet.Attributes;
using Examine.Lucene.Providers;
using Lucene.Net.Analysis.Standard;
using Microsoft.Extensions.Logging;

namespace Examine.Benchmarks
{
    [Config(typeof(NugetConfig))]
    [ThreadingDiagnoser]
    [MemoryDiagnoser]
    public class IndexVersionComparison : ExamineBaseTest
    {
        private readonly List<ValueSet> _valueSets = InitTools.CreateValueSet(100);
        private readonly StandardAnalyzer _analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
        private ILogger<IndexVersionComparison>? _logger;
        private string? _tempBasePath;
        private LuceneIndex? _indexer;

        [GlobalSetup]
        public override void Setup()
        {
            base.Setup();

            _logger = LoggerFactory!.CreateLogger<IndexVersionComparison>();
            _tempBasePath = Path.Combine(Path.GetTempPath(), "ExamineTests");
            _indexer = InitTools.InitializeIndex(this, _tempBasePath, _analyzer, out _);
        }

        [GlobalCleanup]
        public override void TearDown()
        {
            _indexer!.Dispose();
            base.TearDown();
            System.IO.Directory.Delete(_tempBasePath!, true);
        }

        [Benchmark]
        public void IndexItemsNonAsync() => IndexItems(_indexer!, _valueSets);

#if RELEASE
        protected override ILoggerFactory CreateLoggerFactory()
            => Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
#endif

        private static void IndexItems(LuceneIndex indexer, IEnumerable<ValueSet> valueSets)
            => indexer.IndexItems(valueSets);
    }
}
