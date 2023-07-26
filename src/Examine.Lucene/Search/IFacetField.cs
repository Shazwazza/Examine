using System.Collections.Generic;
using Examine.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents a base facet field
    /// </summary>
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
        /// Whether this field is indexed in the Taxonomy index
        /// </summary>
        bool IsTaxonomyIndexed { get; }

        /// <summary>
        /// Extracts the facets from the field
        /// </summary>
        /// <param name="facetExtractionContext"></param>
        /// <returns>Returns the facets for this field</returns>
        IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(IFacetExtractionContext facetExtractionContext);
    }
}
