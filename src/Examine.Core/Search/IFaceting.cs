using System;

namespace Examine.Search
{
    /// <summary>
    /// Allows for selecting facets to return in your query
    /// </summary>
    public interface IFaceting
    {
        /// <summary>
        /// Allows for selecting facets to return in your query
        /// </summary>
        /// <param name="facets"></param>
        /// <returns></returns>
        IQueryExecutor WithFacets(Action<IFacetOperations> facets);
    }
}
