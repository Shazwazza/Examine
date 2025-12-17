using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Context for Extracting the Facets for a field
    /// </summary>
    public interface IFacetExtractionContext
    {
        /// <summary>
        /// Facet Collector
        /// </summary>
        FacetsCollector FacetsCollector { get; }

        /// <summary>
        /// Facet Configuration
        /// </summary>
        FacetsConfig FacetConfig { get; }

        /// <summary>
        /// Index Searcher Reference
        /// </summary>
        ISearcherReference SearcherReference { get; }

        /// <summary>
        /// SortedSetReaderState (Non taxonomy indexed)
        /// </summary>
        SortedSetDocValuesReaderState? SortedSetReaderState { get; }

        /// <summary>
        /// Drill Sideways Result Facets
        /// </summary>
        Facets? DrillSidewaysResultFacets { get; }

        /// <summary>
        /// Get the facet counts for the faceted field
        /// </summary>
        /// <param name="facetIndexFieldName">The name of the field the facet data is stored in</param>
        /// <param name="isTaxonomyIndexed">Whether the facet is stored in the Taxonomy index</param>
        /// <returns></returns>
        Facets GetFacetCounts(string facetIndexFieldName, bool isTaxonomyIndexed);
    }
}
