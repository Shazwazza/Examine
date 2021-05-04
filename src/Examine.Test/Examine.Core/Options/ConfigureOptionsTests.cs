using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Examine.Lucene;
using Examine.Lucene.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Examine.Test.Examine.Core.Options
{
    [TestFixture]
    public class ConfigureOptionsTests
    {
        [Test]
        public async Task Can_Configure_Named_Options()
        {
            IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                    services
                        .AddHostedService<TestService>()
                        .AddExamine()
                        .AddExamineLuceneIndex(
                            "TestIndex",
                            new DirectoryInfo(
                                Path.Combine(
                                    hostContext.HostingEnvironment.ContentRootPath,
                                    "Examine")))
                        .ConfigureOptions<MyIndexOptions>())                
                .Build();

            await host.StartAsync();
            await host.StopAsync();
        }

        private class MyIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
        {
            public void Configure(string name, LuceneDirectoryIndexOptions options)
            {
                if (name != "TestIndex")
                {
                    return;
                }

                // replace with ram dir
                options.IndexDirectory = new RandomIdRAMDirectory();

                // customize some fields
                if (options.FieldDefinitions == null)
                {
                    options.FieldDefinitions = new FieldDefinitionCollection();
                }
                options.FieldDefinitions.AddOrUpdate(
                    new FieldDefinition("lat", FieldDefinitionTypes.Double));
                options.FieldDefinitions.AddOrUpdate(
                    new FieldDefinition("lng", FieldDefinitionTypes.Double));
            }

            // This won't be called, but is required for the interface
            public void Configure(LuceneDirectoryIndexOptions options) { }
        }

        private class TestService : IHostedService
        {
            private readonly IExamineManager _examineManager;

            public TestService(IExamineManager examineManager) => _examineManager = examineManager;

            public Task StartAsync(CancellationToken cancellationToken)
            {
                IIndex index = _examineManager.GetIndex("TestIndex");

                var luceneIndex = index as LuceneIndex;
                Assert.IsNotNull(luceneIndex);

                using (luceneIndex.WithThreadingMode(IndexThreadingMode.Synchronous))
                {
                    luceneIndex.CreateIndex();
                    luceneIndex.IndexItem(
                        ValueSet.FromObject(
                            Guid.NewGuid().ToString(),
                            "locations",
                            new { nodeName = "Zanzibar", bodyText = "Zanzibar is in Africa", lat = 6.1357, lng = 39.3621 }));

                    var results = luceneIndex.Searcher
                        .CreateQuery("locations")
                        .RangeQuery<double>(new[] { "lat" }, 6.00, null)
                        .Execute();

                    Assert.AreEqual(1, results.TotalItemCount);

                    var ramDir = luceneIndex.GetLuceneDirectory() as RandomIdRAMDirectory;
                    Assert.IsNotNull(ramDir);

                    luceneIndex.FieldDefinitionCollection.TryGetValue("lat", out FieldDefinition def1);
                    Assert.AreEqual(FieldDefinitionTypes.Double, def1.Type);

                    luceneIndex.FieldDefinitionCollection.TryGetValue("lng", out FieldDefinition def2);
                    Assert.AreEqual(FieldDefinitionTypes.Double, def2.Type);

                    return Task.CompletedTask;
                }  
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                _examineManager.Dispose();
                return Task.CompletedTask;
            }
        }
    }
}
