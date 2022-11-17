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
        IFacetValue Facet(string label);
    }
}
