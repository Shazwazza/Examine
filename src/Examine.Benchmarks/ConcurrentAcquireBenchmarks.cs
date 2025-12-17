using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;

namespace Examine.Benchmarks
{
    [MediumRunJob(RuntimeMoniker.Net80)]
    [ThreadingDiagnoser]
    [MemoryDiagnoser]
    public class ConcurrentAcquireBenchmarks : ExamineBaseTest
    {
        private readonly StandardAnalyzer _analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
        private string? _tempBasePath;
        private FSDirectory? _indexDir;
        private IndexWriter? _writer;
        private SearcherManager? _searcherManager;

        [GlobalSetup]
        public override void Setup()
        {
            base.Setup();

            _tempBasePath = Path.Combine(Path.GetTempPath(), "ExamineTests");

            // indexer for lucene
            InitializeAndIndexItems(_tempBasePath, _analyzer, out var indexDir);
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
            _searcherManager?.Dispose();
            _writer?.Dispose();
            _indexDir?.Dispose();

            base.TearDown();

            System.IO.Directory.Delete(_tempBasePath!, true);
        }

        [Params(1, 15, 30, 100)]
        public int ThreadCount { get; set; }

        [Benchmark(Baseline = true)]
        public async Task SimpleMultiThreadLoop()
        {
            var tasks = new List<Task>();

            for (var i = 0; i < ThreadCount; i++)
            {
                tasks.Add(new Task(() => Debug.Write(i)));
            }

            foreach (var task in tasks)
            {
                task.Start();
            }

            await Task.WhenAll(tasks);
        }

        [Benchmark]
        public async Task TestAcquireThreadContention()
        {
            var tasks = new List<Task>();

            for (var i = 0; i < ThreadCount; i++)
            {
                tasks.Add(new Task(() =>
                {
                    var searcher = _searcherManager!.Acquire();
                    try
                    {
                        if (searcher.IndexReader.RefCount > (ThreadCount + 1))
                        {
                            Console.WriteLine(searcher.IndexReader.RefCount);
                        }
                    }
                    finally
                    {
                        _searcherManager.Release(searcher);
                    }
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
        private void InitializeAndIndexItems(
            string tempBasePath,
            Analyzer analyzer,
            out DirectoryInfo indexDir)
        {
            var tempPath = Path.Combine(tempBasePath, Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(tempPath);
            indexDir = new DirectoryInfo(tempPath);
            using var luceneDirectory = FSDirectory.Open(indexDir);
            using var luceneTaxonomyDir = FSDirectory.Open(Path.Combine(tempPath, "Taxonomy"));
            using var indexer = GetTestIndex(luceneDirectory, luceneTaxonomyDir, analyzer);

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
    }
}
