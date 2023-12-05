using System;
using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;
using Lucene.Net.Facet.Taxonomy;

namespace Examine.Lucene.Search
{
    /// <inheritdoc/>
    public class LuceneFacetExtractionContext : IFacetExtractionContext
    {

        /// <inheritdoc/>
        public LuceneFacetExtractionContext(FacetsCollector facetsCollector, ISearcherReference searcherReference, FacetsConfig facetConfig,
            Facets? drillSidewaysResultFacets)
        {
            FacetsCollector = facetsCollector;
            FacetConfig = facetConfig;
            DrillSidewaysResultFacets = drillSidewaysResultFacets;
            SearcherReference = searcherReference;
        }

        /// <inheritdoc/>
        public FacetsCollector FacetsCollector { get; }

        /// <inheritdoc/>
        public FacetsConfig FacetConfig { get; }

        /// <inheritdoc/>
        public Facets? DrillSidewaysResultFacets { get; }

        /// <inheritdoc/>
        public ISearcherReference SearcherReference { get; }

        /// <inheritdoc/>
        public SortedSetDocValuesReaderState? SortedSetReaderState { get; private set; }

        /// <inheritdoc/>
        public virtual Facets GetFacetCounts(string facetIndexFieldName, bool isTaxonomyIndexed)
        {
            if (isTaxonomyIndexed)
            {
                if (SearcherReference is ITaxonomySearcherReference taxonomySearcher)
                {
                    return new FastTaxonomyFacetCounts(facetIndexFieldName, taxonomySearcher.TaxonomyReader, FacetConfig, FacetsCollector);
                }
                throw new InvalidOperationException("Cannot get FastTaxonomyFacetCounts for field not stored in the Taxonomy index");
            }
            else
            {
                if (SortedSetReaderState == null || !SortedSetReaderState.Field.Equals(facetIndexFieldName))
                {
                    SortedSetReaderState = new DefaultSortedSetDocValuesReaderState(SearcherReference.IndexSearcher.IndexReader, facetIndexFieldName);
                }
                return new SortedSetDocValuesFacetCounts(SortedSetReaderState, FacetsCollector);
            }
        }
    }
}
