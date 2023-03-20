using System;
using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;
using Lucene.Net.Index;

namespace Examine.Lucene.Search
{
    public class LuceneFacetExtractionContext : IFacetExtractionContext
    {
        private readonly IndexReader _indexReader;

        private SortedSetDocValuesReaderState _sortedSetReaderState = null;

        public LuceneFacetExtractionContext(FacetsCollector facetsCollector, IndexReader indexReader)
        {
            FacetsCollector = facetsCollector;
            _indexReader = indexReader;
        }

        public FacetsCollector FacetsCollector { get; }

        public virtual SortedSetDocValuesReaderState GetSortedSetReaderState(string facetFieldName)
        {
            if (_sortedSetReaderState == null || !_sortedSetReaderState.Field.Equals(facetFieldName))
            {
                _sortedSetReaderState = new DefaultSortedSetDocValuesReaderState(_indexReader, facetFieldName);
            }
            return _sortedSetReaderState;
        }
    }
}
