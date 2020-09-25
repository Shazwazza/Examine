using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Search
{
    public static class QueryExecutorExtensions
    {
        /// <summary>
        /// Executes the query with skip already applied. Use when you only need a single page of results to save on memory. To obtain additional pages you will need to execute the query again.
        /// </summary>
        /// <param name="skip">Number of results to skip</param>
        /// <param name="take">Number of results to take</param>
        /// <returns></returns>
        public static ISearchResults ExecuteWithSkip(this IQueryExecutor query,int skip, int? take = null)
        {
            if(!(query is IQueryExecutor2 queryExecutor2))
            {
                throw new NotSupportedException("IQueryExecutor2 is not implemented");
            }
            return queryExecutor2.ExecuteWithSkip(skip, take);
        }
    }
}
