using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.Search;

namespace Examine
{
    /// <summary>
    /// An interface representing an Examine Searcher
    /// </summary>
    public interface ISearcher
    {
        string Name { get; }

        /// <summary>
        /// Searches the index
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="maxResults"></param>
        /// <returns>Search Results</returns>
        ISearchResults Search(string searchText, int maxResults = 500);

        /// <summary>
        /// Creates a search criteria instance as required by the implementation
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <returns>
        /// An instance of <see cref="IQueryExecutor"/>
        /// </returns>
        IQuery CreateCriteria(string type = null, BooleanOperation defaultOperation = BooleanOperation.And);
    }
}
