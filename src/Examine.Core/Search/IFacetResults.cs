using System.Collections.Generic;

namespace Examine.Lucene.Search
{
    public interface IFacetResults
    {
        /// <summary>
        /// Facets from the search
        /// </summary>
        IDictionary<string, IFacetResult> Facets { get; }
    }
}
