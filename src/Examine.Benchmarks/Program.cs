using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;

namespace Examine.Benchmarks
{
    public class Program
    {
#if RELEASE
        public static void Main(string[] args) =>
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
#else
        public static async Task Main(string[] args)
        {
            var bench = new SearchVersionComparison();
            try
            {
                bench.Setup();
                //await Threads100(bench);
                await Threads1(bench);
            }
            finally
            {
                bench.TearDown();
            }
        }
#endif
        // Call your function here. 

#if LocalBuild
        private static async Task Threads100(ConcurrentSearchBenchmarks bench)
        {
            bench.ThreadCount = 50;
            //bench.MaxResults = 10;

            for (var i = 0; i < 100; i++)
            {
                await bench.ExamineStandard();
            }
        }

        private static async Task Threads1(SearchVersionComparison bench)
        {
            bench.ThreadCount = 1;
            //bench.MaxResults = 10;

            for (var i = 0; i < 100; i++)
            {
                await bench.ConcurrentSearch();
            }
        } 
#endif
    }
}
