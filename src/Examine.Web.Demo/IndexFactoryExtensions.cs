using Lucene.Net.Facet;
using Examine.Lucene.Suggest;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
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
            taxonomyFacetIndexFacetsConfig.SetIndexFieldName("AddressState", "AddressState");

            taxonomyFacetIndexFacetsConfig.SetIndexFieldName("AddressStateCity", "AddressStateCity");
            taxonomyFacetIndexFacetsConfig.SetHierarchical("AddressStateCity", true);
            taxonomyFacetIndexFacetsConfig.SetMultiValued("AddressStateCity", false);

            taxonomyFacetIndexFacetsConfig.SetIndexFieldName("Tags", "Tags");
            taxonomyFacetIndexFacetsConfig.SetMultiValued("Tags", true);

            services.AddExamineLuceneIndex(
                "TaxonomyFacetIndex",
                facetsConfig: taxonomyFacetIndexFacetsConfig);

            var facetIndexFacetsConfig = new FacetsConfig();

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
