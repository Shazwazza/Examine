using System;
using System.IO;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Lucene.Net.Analysis.Standard;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Examine.Web.Demo
{
    /// <summary>
    /// Creates the indexes
    /// </summary>
    public static class IndexFactoryExtensions
    {
        public static IServiceCollection CreateIndexes(this IServiceCollection services)
        {
            services.AddExamineLuceneIndex("Simple2Indexer");

            services.AddExamineLuceneIndex("SecondIndexer");

            services.AddExamineLuceneMultiSearcher(
                "MultiIndexSearcher",
                new[] { "Simple2Indexer", "SecondIndexer" });

            services.ConfigureOptions<ConfigureIndexOptions>();

            return services;
        }
    }

    /// <summary>
    /// Configures the index options to construct the Examine indexes
    /// </summary>
    public sealed class ConfigureIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
    {
        public void Configure(string name, LuceneDirectoryIndexOptions options)
        {
            switch (name)
            {
                case "Simple2Indexer":
                    options.Analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
                    break;
            }
        }

        public void Configure(LuceneDirectoryIndexOptions options)
            => throw new NotImplementedException("This is never called and is just part of the interface");
    }
}
