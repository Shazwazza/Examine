using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.SearchCriteria;

namespace Examine
{
    /// <summary>
    /// An interface representing an Examine Searcher with a typed result
    /// </summary>
    /// <typeparam name="TResults"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TSearchCriteria"></typeparam>
    public interface ISearcher<out TResults, TResult, out TSearchCriteria>
        where TResults : ISearchResults<TResult>
        where TResult : ISearchResult
        where TSearchCriteria : ISearchCriteria
    {
        /// <summary>
        /// Simple search method which should default to searching content nodes
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is the same as Search but with a typed result
        /// </remarks>
        TResults Find(string searchText, bool useWildcards);
        /// <summary>
        /// Searches the data source using the Examine Fluent API
        /// </summary>
        /// <param name="searchParams">The fluent API search.</param>
        /// <returns></returns>
        /// <remarks>
        /// This is the same as Search but with a typed result
        /// </remarks>
        TResults Find(ISearchCriteria searchParams);

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <returns>A blank SearchCriteria</returns>
        TSearchCriteria CreateCriteria(string type = null, BooleanOperation defaultOperation = BooleanOperation.And);
    }

    /// <summary>
    /// An interface representing an Examine Searcher
    /// </summary>
    public interface ISearcher
    {
        /// <summary>
        /// Searches the specified search text in all fields of the index
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="useWildcards">if set to <c>true</c> the search will use wildcards.</param>
        /// <returns>Search Results</returns>
        ISearchResults Search(string searchText, bool useWildcards);

        /// <summary>
        /// Searches using the specified search query parameters
        /// </summary>
        /// <param name="searchParameters">The search parameters.</param>
        /// <returns>Search Results</returns>
        ISearchResults Search(ISearchCriteria searchParameters);

        /// <summary>
        /// Creates a search criteria instance as required by the implementation
        /// </summary>
        /// <returns></returns>
        ISearchCriteria CreateSearchCriteria();

        /// <summary>
        /// Creates a search criteria instance as required by the implementation
        /// </summary>
        /// <param name="defaultOperation"></param>
        /// <returns></returns>
        ISearchCriteria CreateSearchCriteria(BooleanOperation defaultOperation);

        /// <summary>
        /// Creates a search criteria instance as required by the implementation
        /// </summary>
        /// <param name="type">The type of index (i.e. Media or Content )</param>
        ISearchCriteria CreateSearchCriteria(string type);

        /// <summary>
        /// Creates a search criteria instance as required by the implementation
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <returns>
        /// An instance of <see cref="Examine.SearchCriteria.ISearchCriteria"/>
        /// </returns>
        ISearchCriteria CreateSearchCriteria(string type, BooleanOperation defaultOperation);
    }
}
