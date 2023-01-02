using System.Collections.Generic;
using Examine.Search;
using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;

namespace Examine.Lucene.Search
{
    public interface IFacetField
    {
        /// <summary>
        /// The field name
        /// </summary>
        string Field { get; }

        /// <summary>
        /// The field to get the facet field from
        /// </summary>
        string FacetField { get; }

        /// <summary>
        /// Extracts the facets from the field
        /// </summary>
        /// <param name="facetsCollector"></param>
        /// <param name="sortedSetReaderState"></param>
        /// <param name="facets"></param>
        void ExtractFacets(FacetsCollector facetsCollector, SortedSetDocValuesReaderState sortedSetReaderState, Dictionary<string, IFacetResult> facets);
    }
}
