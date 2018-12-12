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
        /// Searches the index
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public abstract ISearchResults Search(string searchText, int maxResults = 500);

        /// <summary>
        /// Searches using the specified search query parameters
        /// </summary>
        /// <param name="searchParameters">The search parameters.</param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public abstract ISearchResults Search(ISearchCriteria searchParameters, int maxResults = 500);

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <returns></returns>
        public abstract ISearchCriteria CreateCriteria();

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <returns>A blank SearchCriteria</returns>
        public abstract ISearchCriteria CreateCriteria(string type);

        ///<summary>
        /// Creates an instance of SearchCriteria for the provider
        ///</summary>
        ///<param name="defaultOperation"></param>
        ///<returns></returns>
		public abstract ISearchCriteria CreateCriteria(BooleanOperation defaultOperation);

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <returns>A blank SearchCriteria</returns>
		public abstract ISearchCriteria CreateCriteria(string type, BooleanOperation defaultOperation);
        
    }
}
