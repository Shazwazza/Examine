using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.QueryParsers.Flexible.Standard;
using Lucene.Net.QueryParsers.Flexible.Standard.Config;
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

namespace Examine.Test.Examine.Lucene.Search
{
    [ShortRunJob]
    [ThreadingDiagnoser]
    [MemoryDiagnoser]
    [DotNetCountersDiagnoser]
    [CPUUsageDiagnoser]
    public class ConcurrentSearchBenchmarks : ExamineBaseTest
    {
        private ILogger<ConcurrentSearchBenchmarks> _logger;
        private string _tempBasePath;
        private FSDirectory _luceneDir;
        private TestIndex _indexer;

        [GlobalSetup]
        public override void Setup()
        {
            base.Setup();

            _logger = LoggerFactory.CreateLogger<ConcurrentSearchBenchmarks>();
            _tempBasePath = Path.Combine(Path.GetTempPath(), "ExamineTests");

            var tempPath = Path.Combine(_tempBasePath, Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(tempPath);
            var temp = new DirectoryInfo(tempPath);
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            _luceneDir = FSDirectory.Open(temp);
            _indexer = GetTestIndex(_luceneDir, analyzer);

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

            _indexer.IndexItems(valueSets);
        }

        [GlobalCleanup]
        public override void TearDown()
        {
            _indexer.Dispose();
            _luceneDir.Dispose();

            base.TearDown();

            System.IO.Directory.Delete(_tempBasePath, true);
        }

        [Params(1, 5, 15)]
        public int ThreadCount { get; set; }

        [Benchmark]
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
                    var results = query.Execute();

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

            for (var i = 0; i < ThreadCount; i++)
            {
                tasks.Add(new Task(() =>
                {
                    var parser = new QueryParser(LuceneVersion.LUCENE_48, ExamineFieldNames.ItemIdFieldName, new StandardAnalyzer(LuceneVersion.LUCENE_48));
                    var query = parser.Parse($"{ExamineFieldNames.CategoryFieldName}:content AND nodeName:location*");

                    using var reader = DirectoryReader.Open(_luceneDir);
                    var searcher = new IndexSearcher(reader);
                    var maxDoc = searcher.IndexReader.MaxDoc;
                    var topDocsCollector = TopScoreDocCollector.Create(maxDoc, null, true);

                    searcher.Search(query, topDocsCollector);
                    var topDocs = ((TopScoreDocCollector)topDocsCollector).GetTopDocs(0, QueryOptions.DefaultMaxResults);

                    var totalItemCount = topDocs.TotalHits;

                    var results = new List<ISearchResult>(topDocs.ScoreDocs.Length);
                    for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
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
                    float maxScore = topDocs.MaxScore;

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
