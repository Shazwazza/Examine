namespace Examine.Search
{
    public interface IOrdering : IQueryExecutor
    {
        /// <summary>
        /// Orders the results by the specified fields
        /// </summary>
        /// <param name="fields">The field names.</param>
        /// <returns></returns>
        IOrdering OrderBy(params SortableField[] fields);

        /// <summary>
        /// Orders the results by the specified fields in a descending order
        /// </summary>
        /// <param name="fields">The field names.</param>
        /// <returns></returns>
        IOrdering OrderByDescending(params SortableField[] fields);
    }
}