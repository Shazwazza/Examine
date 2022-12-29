using Lucene.Net.Facet;
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
            services.AddExamineLuceneIndex("MyIndex");

            services.AddExamineLuceneIndex("SyncedIndex");

            var taxonomyFacetIndexFacetsConfig = new FacetsConfig();

            services.AddExamineLuceneTaxonomyIndex(
                "TaxonomyFacetIndex",
                facetsConfig: taxonomyFacetIndexFacetsConfig);

            services.AddExamineLuceneMultiSearcher(
                "MultiIndexSearcher",
                new[] { "MyIndex", "SyncedIndex" },
                facetsConfig: new FacetsConfig());

            services.ConfigureOptions<ConfigureIndexOptions>();

            return services;
        }
    }
}
