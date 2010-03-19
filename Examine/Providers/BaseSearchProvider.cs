using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;
using UmbracoExamine.Core;

namespace UmbracoExamine.Providers
{
    public abstract class BaseSearchProvider : ProviderBase, ISearcher
    {

        #region ISearcher Members

        /// <summary>
        /// Simple search method which should default to searching content nodes
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="maxResults"></param>
        /// <param name="useWildcards"></param>
        /// <returns></returns>
        public abstract IEnumerable<SearchResult> Search(string searchText, int maxResults, bool useWildcards);
        public abstract IEnumerable<SearchResult> Search(ISearchCriteria searchParams);


        #endregion
    }
}
