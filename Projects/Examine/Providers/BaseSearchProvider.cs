using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Configuration.Provider;
using Examine;
using Examine.SearchCriteria;

namespace Examine.Providers
{
    /// <summary>
    /// Abstract search provider with a typed search result
    /// </summary>
    /// <typeparam name="TResults"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TSearchCriteria"></typeparam>
    public abstract class BaseSearchProvider<TResults, TResult, TSearchCriteria> : BaseSearchProvider, ISearcher<TResults, TResult, TSearchCriteria>
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
        public abstract TResults Find(string searchText, bool useWildcards);
        /// <summary>
        /// Searches the data source using the Examine Fluent API
        /// </summary>
        /// <param name="searchParams">The fluent API search.</param>
        /// <returns></returns>
        /// <remarks>
        /// This is the same as Search but with a typed result
        /// </remarks>
        public abstract TResults Find(ISearchCriteria searchParams);

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <returns>A blank SearchCriteria</returns>
        public abstract TSearchCriteria CreateCriteria(string type = null, BooleanOperation defaultOperation = BooleanOperation.And);
    }

    ///<summary>
    /// Abstract search provider object
    ///</summary>
    public abstract class BaseSearchProvider : ProviderBase, ISearcher
    {
        #region ISearcher Members

        /// <summary>
        /// Simple search method which should default to searching content nodes
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <returns></returns>
        [Obsolete("Use the Find method on a strongly typed search provider instead for strongly typed search results")]
        public abstract ISearchResults Search(string searchText, bool useWildcards);
        /// <summary>
        /// Searches the data source using the Examine Fluent API
        /// </summary>
        /// <param name="searchParams">The fluent API search.</param>
        /// <returns></returns>
        [Obsolete("Use the Find method on a strongly typed search provider instead for strongly typed search results")]
        public abstract ISearchResults Search(ISearchCriteria searchParams);

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use the CreateCriteria method on a strongly typed search provider instead for strongly typed search criteria")]
        public abstract ISearchCriteria CreateSearchCriteria();

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <returns>A blank SearchCriteria</returns>
        [Obsolete("Use the CreateCriteria method on a strongly typed search provider instead for strongly typed search criteria")]
        public abstract ISearchCriteria CreateSearchCriteria(string type);

        ///<summary>
        /// Creates an instance of SearchCriteria for the provider
        ///</summary>
        ///<param name="defaultOperation"></param>
        ///<returns></returns>
        [Obsolete("Use the CreateCriteria method on a strongly typed search provider instead for strongly typed search criteria")]
        public abstract ISearchCriteria CreateSearchCriteria(BooleanOperation defaultOperation);

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <returns>A blank SearchCriteria</returns>
        [Obsolete("Use the CreateCriteria method on a strongly typed search provider instead for strongly typed search criteria")]
        public abstract ISearchCriteria CreateSearchCriteria(string type, BooleanOperation defaultOperation);

        #endregion
    }
}