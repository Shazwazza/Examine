using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Configuration.Provider;
using Examine;
using Examine.Search;

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

        /// <inheritdoc />
		public abstract IQuery CreateQuery(string type = null, BooleanOperation defaultOperation = BooleanOperation.And);
        
    }
}
