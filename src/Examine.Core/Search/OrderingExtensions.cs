using System;

namespace Examine.Search
{
    public static class OrderingExtensions
    {
        /// <summary>
        /// Allows for selecting facets to return in your query
        /// </summary>
        /// <param name="facets"></param>
        /// <returns></returns>
        public static IQueryExecutor WithFacets(this IOrdering ordering, Action<IFacetOperations> facets)
        {
            if (ordering is IFaceting faceting)
            {
                return faceting.WithFacets(facets);
            }

            throw new NotSupportedException("The current implementation of Examine does not support faceting");
        }
    }
}
