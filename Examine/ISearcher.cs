using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.SearchCriteria;

namespace Examine
{
    /// <summary>
    /// An interface representing an Examine Searcher
    /// </summary>
    public interface ISearcher
    {
        /// <summary>
        /// Searches the specified search text in all fields of the index
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="maxResults">The max number of results.</param>
        /// <param name="useWildcards">if set to <c>true</c> the search will use wildcards.</param>
        /// <returns>Search Results</returns>
        IEnumerable<SearchResult> Search(string searchText, int maxResults, bool useWildcards);
        /// <summary>
        /// Searches using the specified search query parameters
        /// </summary>
        /// <param name="searchParameters">The search parameters.</param>
        /// <returns>Search Results</returns>
        IEnumerable<SearchResult> Search(ISearchCriteria searchParameters);

        /// <summary>
        /// Creates a search criteria instance as required by the implementation
        /// </summary>
        /// <param name="maxResults">The max number of results.</param>
        /// <param name="type">The type of data in the index.</param>
        /// <returns>An instance of <see cref="Examine.SearchCriteria.ISearchCriteria"/></returns>
        ISearchCriteria CreateSearchCriteria(int maxResults, IndexType type);
    }
}
