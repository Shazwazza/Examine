using System.Collections.Generic;

namespace Examine.Search
{
    /// <summary>
    /// Represents a search result containing facets
    /// </summary>
    public interface IFacetResults
    {
        /// <summary>
        /// Facets from the search
        /// </summary>
        IReadOnlyDictionary<string, IFacetResult> Facets { get; }
    }
}
