using Lucene.Net.Facet.SortedSet;
using Lucene.Net.Facet;
using System;

namespace Examine.Lucene.Search
{
    /// <inheritdoc/>
    public class LuceneFacetExtractionContext : IFacetExtractionContext
    {

        private SortedSetDocValuesReaderState _sortedSetReaderState = null;

        /// <inheritdoc/>
        public LuceneFacetExtractionContext(FacetsCollector facetsCollector, ISearcherReference searcherReference, FacetsConfig facetConfig)
        {
            FacetsCollector = facetsCollector;
            FacetConfig = facetConfig;
            SearcherReference = searcherReference;
        }

        /// <inheritdoc/>
        public FacetsCollector FacetsCollector { get; }

        /// <inheritdoc/>
        public FacetsConfig FacetConfig { get; }

        /// <inheritdoc/>
        public ISearcherReference SearcherReference { get; }

        /// <inheritdoc/>
        public virtual Facets GetFacetCounts(string facetIndexFieldName, bool isTaxonomyIndexed)
        {
            if (isTaxonomyIndexed)
            {
                throw new NotSupportedException("Taxonomy Index not supported");
            }
            else
            {
                if (_sortedSetReaderState == null || !_sortedSetReaderState.Field.Equals(facetIndexFieldName))
                {
                    _sortedSetReaderState = new DefaultSortedSetDocValuesReaderState(SearcherReference.IndexSearcher.IndexReader, facetIndexFieldName);
                }
                return new SortedSetDocValuesFacetCounts(_sortedSetReaderState, FacetsCollector);
            }
        }
    }
}
