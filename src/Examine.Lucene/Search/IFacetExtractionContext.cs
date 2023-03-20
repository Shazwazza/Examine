using Lucene.Net.Facet;

namespace Examine.Lucene.Search
{
    public interface IFacetExtractionContext
    {
        FacetsCollector FacetsCollector { get; }

        FacetsConfig FacetConfig { get; }

        Facets GetFacetCounts(string facetFieldName, bool isTaxonomyIndexed);
    }
}
