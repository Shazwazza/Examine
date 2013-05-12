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
        string SearchIndexType { get; }

        /// <summary>
        /// Passes a text string which is preformatted for the underlying search API. Examine will not modify this
        /// </summary>
        /// <remarks>
        /// This allows a developer to completely bypass and Examine logic and comprise their own query text which they are passing in.
        /// It means that if the search is too complex to achieve with the fluent API, or too dynamic to achieve with a static language
        /// the provider can still handle it.
        /// </remarks>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        ISearchCriteria RawQuery(string query);        
    }
}
