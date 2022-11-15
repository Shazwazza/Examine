using System.Collections.Generic;

namespace Examine.Lucene.Search
{
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
