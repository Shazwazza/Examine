using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
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
            services.AddExamineLuceneIndex<LuceneIndex, SyncedFileSystemDirectoryFactory>("MyIndex");

            services.AddExamineLuceneIndex<LuceneIndex, SyncedFileSystemDirectoryFactory>("SyncedIndex");

            services.AddExamineLuceneMultiSearcher(
                "MultiIndexSearcher",
                new[] { "MyIndex", "SyncedIndex" });

            services.ConfigureOptions<ConfigureIndexOptions>();

            return services;
        }
    }
}
