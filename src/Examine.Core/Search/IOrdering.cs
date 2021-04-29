using System.Collections.Generic;

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

        /// <summary>
        /// Return only the specified fields. Use <see cref="SelectFields(ISet{string})"/> when possible as internally a new HashSet is created on each call
        /// </summary>
        /// <param name="fieldNames">The field names for fields to load</param>
        /// <returns></returns>
        IOrdering SelectFields(params string[] fieldNames);

        /// <summary>
        /// Return only the specified fields
        /// </summary>
        /// <param name="fieldNames">The field names for fields to load</param>
        /// <returns></returns>
        IOrdering SelectFields(ISet<string> fieldNames);

        /// <summary>
        /// Return only the specified field. Use <see cref="SelectFields(ISet{string})"/> when possible as internally a new HashSet is created on each call
        /// </summary>
        /// <param name="fieldNames">The field name of the field to load</param>
        /// <returns></returns>
        IOrdering SelectField(string fieldName);

        /// <summary>
        /// Return only the first field in the index
        /// </summary>
        /// <remarks>This should be the __NodeId field as it should be first in the index</remarks>
        /// <returns></returns>
        IOrdering SelectFirstFieldOnly();

        /// <summary>
        /// Return all fields in the index
        /// </summary>
        /// <returns></returns>
        IOrdering SelectAllFields();
    }
}