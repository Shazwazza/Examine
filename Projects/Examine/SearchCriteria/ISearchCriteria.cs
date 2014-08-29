using System;
using System.Collections.Generic;

namespace Examine.SearchCriteria
{
    /// <summary>
    /// Defines the query parameters for the search
    /// </summary>
    public interface ISearchCriteria<out TBoolOp, out TSearchCriteria> : IQuery<TBoolOp>, ISearchCriteria
        where TBoolOp : IBooleanOperation
        where TSearchCriteria : ISearchCriteria<TBoolOp, TSearchCriteria>
    {
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
        new TSearchCriteria RawQuery(string query);

        /// <summary>
        /// Sets the max count for the result
        /// </summary>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        new TSearchCriteria MaxCount(int maxCount);

    }

    /// <summary>
    /// Defines the query parameters for the search
    /// </summary>
    public interface ISearchCriteria : IQuery
    {
        /// <summary>
        /// Indicates the max number of results to return
        /// </summary>
        int MaxResults { get; }

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

        /// <summary>
        /// Sets the max count for the result
        /// </summary>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        ISearchCriteria MaxCount(int maxCount);

    }
}
