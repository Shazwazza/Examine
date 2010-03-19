using System;
using System.Collections.Generic;

namespace Examine.SearchCriteria
{
    public interface ISearchCriteria : IQuery
    {
        /// <summary>
        /// Specifies the number of results to return
        /// </summary>
        int MaxResults { get; }
        /// <summary>
        /// If <c>true</c> the total number of matches (hits) will be calculated
        /// </summary>
        bool IncludeHitCount { get; set; }
        /// <summary>
        /// Number of matches (hits) from this search criteria
        /// </summary>
        int TotalHits { get; }
        /// <summary>
        /// Indicates the type of data to search on
        /// </summary>
        IndexType SearchIndexType { get; }
    }
}
