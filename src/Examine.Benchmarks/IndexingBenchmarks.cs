using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Examine.Test;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Diagnostics.Tracing.AutomatedAnalysis;
using static Lucene.Net.Util.Packed.PackedInt32s;

namespace Examine.Benchmarks
{
    [MediumRunJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser]
    public class IndexingBenchmarks : ExamineBaseTest
    {
        private readonly StandardAnalyzer _analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
        private string? _tempBasePath;
        private TestIndex? _indexer1;
        private TestIndex? _indexer2;

        [GlobalSetup]
        public override void Setup()
        {
            base.Setup();

            _tempBasePath = Path.Combine(Path.GetTempPath(), "ExamineTests");

            var tempPath = Path.Combine(_tempBasePath, Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(tempPath);
            var indexDir = new DirectoryInfo(tempPath);
            var luceneDirectory = FSDirectory.Open(indexDir);
            _indexer1 = GetTestIndex(luceneDirectory, _analyzer, reuseDocument: true);
            _indexer2 = GetTestIndex(luceneDirectory, _analyzer, reuseDocument: false);
        }

        [Benchmark(Baseline = true)]
        public void DontReuseDocument() => IndexItems(_indexer2!);

        [Benchmark]
        public void ReuseDocument() => IndexItems(_indexer1!);

        private static void IndexItems(TestIndex indexer)
        {
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
