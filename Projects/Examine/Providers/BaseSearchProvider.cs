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
    ///<summary>
    /// Abstract search provider object
    ///</summary>
    public abstract class BaseSearchProvider : ProviderBase, ISearcher
    {

        /// <summary>
        /// Performs a search with a maximum number of results
        /// </summary>        
        public virtual ISearchResults Search(ISearchCriteria searchParams, int maxResults)
        {
            //returns base method
            return Search(searchParams);
        }

        /// <summary>
        /// A simple search mechanism to search all fields based on an index type.
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <param name="indexType">By default this doesn't have any affect, it is up to inheritors to make this do something</param>
        /// <returns></returns>
        public virtual ISearchResults Search(string searchText, bool useWildcards, string indexType)
        {
            return Search(searchText, useWildcards);
        }

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
		[SecuritySafeCritical]
		public abstract ISearchCriteria CreateSearchCriteria(BooleanOperation defaultOperation);

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <returns>A blank SearchCriteria</returns>
		[SecuritySafeCritical]
		public abstract ISearchCriteria CreateSearchCriteria(string type, BooleanOperation defaultOperation);

        #endregion
    }
}
