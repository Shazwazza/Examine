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
    }
}
