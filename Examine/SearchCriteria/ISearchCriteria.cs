using System;
using System.Collections.Generic;

namespace Examine.SearchCriteria
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISearchCriteria : IQuery
    {
        /// <summary>
        /// Indicates the type of data to search on
        /// </summary>
        IndexType SearchIndexType { get; }

        /// <summary>
        /// Passes a raw search query to the provider to handle
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        ISearchCriteria RawQuery(string query);
    }
}
