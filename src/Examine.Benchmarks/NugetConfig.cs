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

            AddJob(baseJob.WithId("Source"));
            AddJob(baseJob.WithNuGet("Examine", "3.3.0").WithId("3.3.0").WithArguments([new MsBuildArgument("/p:LocalBuild=false")]));
            AddJob(baseJob.WithNuGet("Examine", "3.3.0").WithId("3.3.0").WithArguments([new MsBuildArgument("/p:LocalBuild=false")]));
            AddJob(baseJob.WithNuGet("Examine", "3.2.1").WithId("3.2.1").WithArguments([new MsBuildArgument("/p:LocalBuild=false")]));
            AddJob(baseJob.WithNuGet("Examine", "3.1.0").WithId("3.1.0").WithArguments([new MsBuildArgument("/p:LocalBuild=false")]));
            AddJob(baseJob.WithNuGet("Examine", "3.0.1").WithId("3.0.1").WithArguments([new MsBuildArgument("/p:LocalBuild=false")]));
        }
    }
}
