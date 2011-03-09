using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;
using Examine;
using Examine.SearchCriteria;

namespace Examine.Providers
{
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
        public abstract ISearchResults Search(string searchText, bool useWildcards);
        /// <summary>
        /// Searches the data source using the Examine Fluent API
        /// </summary>
        /// <param name="searchParams">The fluent API search.</param>
        /// <returns></returns>
        public abstract ISearchResults Search(ISearchCriteria searchParams);

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <returns></returns>
        public abstract ISearchCriteria CreateSearchCriteria();

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <returns>A blank SearchCriteria</returns>
        public abstract ISearchCriteria CreateSearchCriteria(string type);

        ///<summary>
        /// Creates an instance of SearchCriteria for the provider
        ///</summary>
        ///<param name="defaultOperation"></param>
        ///<returns></returns>
        public abstract ISearchCriteria CreateSearchCriteria(BooleanOperation defaultOperation);

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <returns>A blank SearchCriteria</returns>
        public abstract ISearchCriteria CreateSearchCriteria(string type, BooleanOperation defaultOperation);

        #endregion
    }
}
