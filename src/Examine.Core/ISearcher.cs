using Examine.Search;

namespace Examine
{
    /// <summary>
    /// An interface representing an Examine Searcher
    /// </summary>
    public interface ISearcher
    {
        /// <summary>
        /// The searchers name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Searches the index
        /// </summary>
        /// <param name="searchText">The search text or a native query</param>
        /// <param name="options"></param>
        /// <returns>Search Results</returns>
        public ISearchResults Search(string searchText, QueryOptions? options = null);

        /// <summary>
        /// Creates a search criteria instance as required by the implementation
        /// </summary>
        /// <param name="category">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <returns>
        /// An instance of <see cref="IQueryExecutor"/>
        /// </returns>
        public IQuery CreateQuery(string? category = null, BooleanOperation defaultOperation = BooleanOperation.And);
    }
}
