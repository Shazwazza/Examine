using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace Examine.Benchmarks
{
    public class NugetConfig : ManualConfig
    {
        public NugetConfig()
        {
            var baseJob = Job.ShortRun
                .WithRuntime(CoreRuntime.Core80);

            string[] targetVersions = ["3.3.0", "3.2.1", "3.1.0", "3.0.1"];

            AddJob(baseJob.WithId("Source"));

            foreach(var version in targetVersions)
            {
                AddJob(baseJob
                    .WithMsBuildArguments($"/p:SciVersion={version}", "/p:LocalBuild=false")
                    .WithId(version));
            }
        }
    }
}
