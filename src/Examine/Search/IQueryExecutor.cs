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


        /// <summary>
        /// Executes the query with skip already applied. Use when you only need a single page of results to save on memory. To obtain additional pages you will need to execute the query again.
        /// </summary>
        /// <param name="skip">Number of results to skip</param>
        /// <param name="take">Number of results to take</param>
        /// <returns></returns>
        ISearchResults ExecuteWithSkip(int skip, int? take = null);

        /// <summary>
        /// Executes the query with skip already applied. Use when you only need a single page of results to save on memory. To obtain additional pages you will need to execute the query again.
        /// </summary>
        /// <param name="skip">Number of results to skip</param>
        /// <param name="take">Number of results to take</param>
        /// <returns></returns>
        ISearchResults Execute(int take, int skip);
    }
}
