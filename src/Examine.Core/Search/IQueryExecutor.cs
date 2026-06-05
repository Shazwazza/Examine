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
        ISearchResults Execute(QueryOptions? options = null);
    }
}
