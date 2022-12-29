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
            taxonomyFacetIndexFacetsConfig.SetIndexFieldName("dimaddressstate", "AddressState");

            services.AddExamineLuceneTaxonomyIndex(
                "TaxonomyFacetIndex",
                facetsConfig: taxonomyFacetIndexFacetsConfig);


            var facetIndexFacetsConfig = new FacetsConfig();;

            services.AddExamineLuceneIndex(
                "FacetIndex",
                facetsConfig: facetIndexFacetsConfig);


            services.AddExamineLuceneMultiSearcher(
                "MultiIndexSearcher",
                new[] { "MyIndex", "SyncedIndex", "FacetIndex" },
                facetsConfig: new FacetsConfig());

            services.ConfigureOptions<ConfigureIndexOptions>();

            return services;
        }
    }
}
