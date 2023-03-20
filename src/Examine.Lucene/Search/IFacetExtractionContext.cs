using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;

namespace Examine.Lucene.Search
{
    public interface IFacetExtractionContext
    {
        FacetsCollector FacetsCollector { get; }

        SortedSetDocValuesReaderState GetSortedSetReaderState(string facetFieldName);
    }
}
