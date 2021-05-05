using System.IO;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

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

            return services;
        }
    }
}
