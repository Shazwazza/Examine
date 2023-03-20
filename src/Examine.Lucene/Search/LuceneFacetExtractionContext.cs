using System;
using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;
using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Index;

namespace Examine.Lucene.Search
{
    public class LuceneFacetExtractionContext : IFacetExtractionContext
    {
        private readonly IndexReader _indexReader;

        private SortedSetDocValuesReaderState _sortedSetReaderState = null;

        public LuceneFacetExtractionContext(FacetsCollector facetsCollector, ISearcherReference searcherReference, FacetsConfig facetConfig)
        {
            FacetsCollector = facetsCollector;
            FacetConfig = facetConfig;
            SearcherReference = searcherReference;
        }

        public FacetsCollector FacetsCollector { get; }

        public FacetsConfig FacetConfig { get; }

        public ISearcherReference SearcherReference { get; }

        public virtual SortedSetDocValuesReaderState GetSortedSetReaderState(string facetFieldName)
        {
            if (_sortedSetReaderState == null || !_sortedSetReaderState.Field.Equals(facetFieldName))
            {
                _sortedSetReaderState = new DefaultSortedSetDocValuesReaderState(SearcherReference.IndexSearcher.IndexReader, facetFieldName);
            }
            return _sortedSetReaderState;
        }

        public virtual Facets GetTaxonomyFacetCounts(string facetIndexFieldName)
        {
            if (SearcherReference is ITaxonomySearcherReference taxonomySearcher)
            {
                return new FastTaxonomyFacetCounts(facetIndexFieldName, taxonomySearcher.TaxonomyReader, FacetConfig, FacetsCollector);
            }
            throw new InvalidOperationException("Cannot get FastTaxonomyFacetCounts for field not stored in the Taxonomy index");
        }
    }
}
