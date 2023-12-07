using System.Collections.Generic;

namespace Examine.Search
{
    /// <summary>
    /// Represents a facet results consisting of <see cref="IFacetValue"/>
    /// </summary>
    public interface IFacetResult : IEnumerable<IFacetValue>
    {
        /// <summary>
        /// Gets the facet for a label
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        IFacetValue? Facet(string label);

        /// <summary>
        /// Trys to get a facet value for a label
        /// </summary>
        /// <param name="label"></param>
        /// <param name="facetValue"></param>
        /// <returns></returns>
        bool TryGetFacet(string label, out IFacetValue? facetValue);
    }
}
