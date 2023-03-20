using System.Collections.Generic;
using Examine.Lucene.Search;
using Examine.Search;

namespace Examine.Lucene.Indexing
{
    public interface IIndexFacetValueType
    {
        /// <summary>
        /// Extracts the facets from the field
        /// </summary>
        /// <param name="facetsCollector"></param>
        /// <param name="sortedSetReaderState"></param>
        /// <param name="field"></param>
        /// <returns>A dictionary of facets for this field</returns>
        IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(IFacetExtractionContext facetExtractionContext, IFacetField field);
    }
}
