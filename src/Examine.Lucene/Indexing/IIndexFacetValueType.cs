using System.Collections.Generic;
using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;

namespace Examine.Lucene.Indexing
{
    public interface IIndexFacetValueType
    {
        /// <summary>
        /// Extracts the facets from the field
        /// </summary>
        /// <param name="facetsCollector"></param>
        /// <param name="sortedSetReaderState"></param>
        /// <param name="facets"></param>
        /// <param name="field"></param>
        void ExtractFacets(FacetsCollector facetsCollector, SortedSetDocValuesReaderState sortedSetReaderState, Dictionary<string, IFacetResult> facets, IFacetField field);
    }
}
