namespace Examine.Search
{
    /// <summary>
    /// Executes a query
    /// </summary>
    public interface IQueryExecutor
    {
        /// <summary>
        /// Executes the query
        /// </summary>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        ISearchResults Execute(int maxResults = 500);
    }
}
