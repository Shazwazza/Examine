using System.Collections.Generic;

namespace Examine.Search
{
    public interface IFacetResults
    {
        /// <summary>
        /// Facets from the search
        /// </summary>
        IReadOnlyDictionary<string, IFacetResult> Facets { get; }
    }
}
