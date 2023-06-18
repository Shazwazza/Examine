using System.Collections.Generic;
using Examine.Lucene.Search;
using Examine.Search;

namespace Examine.Lucene.Indexing
{
    /// <summary>
    /// Represents a facet index value type
    /// </summary>
    public interface IIndexFacetValueType
    {
        /// <summary>
        /// Extracts the facets from the field
        /// </summary>
        /// <param name="facetExtractionContext"></param>
        /// <param name="field"></param>
        /// <returns>A dictionary of facets for this field</returns>
        IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(IFacetExtractionContext facetExtractionContext, IFacetField field);
    }
}
