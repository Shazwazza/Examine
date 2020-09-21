using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Search
{
    public interface IFieldSelectableQuery : IQuery
    {
        /// <summary>
        /// Return only the specified fields. Use <see cref="SelectFields(ISet{string})"/> when possible as internally a new HashSet is created on each call
        /// </summary>
        /// <param name="fieldNames">The field names for fields to load</param>
        /// <returns></returns>
        IBooleanOperation SelectFields(params string[] fieldNames);

        /// <summary>
        /// Return only the specified fields
        /// </summary>
        /// <param name="fieldNames">The field names for fields to load</param>
        /// <returns></returns>
        IBooleanOperation SelectFields(ISet<string> fieldNames);

        /// <summary>
        /// Return only the specified field. Use <see cref="SelectFields(ISet{string})"/> when possible as internally a new HashSet is created on each call
        /// </summary>
        /// <param name="fieldNames">The field name of the field to load</param>
        /// <returns></returns>
        IBooleanOperation SelectField(string fieldName);

        /// <summary>
        /// Return only the specified fields Use <see cref="SelectFields(ISet{string})"/> when possible as internally a new HashSet is created on each call
        /// </summary>
        /// <remarks>The Id field will also be retrieved as it is a required field.</remarks>
        /// <param name="fieldNames">The field names for fields to load. Key should be the field name, value should be null</param>
        /// <returns></returns>
        [Obsolete("Use SelectFields(ISet<string> fieldNames) to reduce allocations")]
        IBooleanOperation SelectFields(Hashtable fieldNames);

        /// <summary>
        /// Return only the first field in the index
        /// </summary>
        /// <remarks>This should be the __NodeId field as it should be first in the index</remarks>
        /// <returns></returns>
        IBooleanOperation SelectFirstFieldOnly();

        /// <summary>
        /// Return all fields in the index
        /// </summary>
        /// <returns></returns>
        IBooleanOperation SelectAllFields();

        /// <summary>
        /// Passes a text string which is preformatted for the underlying search API. Examine will not modify this
        /// </summary>
        /// <remarks>
        /// This allows a developer to completely bypass and Examine logic and comprise their own query text which they are passing in.
        /// It means that if the search is too complex to achieve with the fluent API, or too dynamic to achieve with a static language
        /// the provider can still handle it.
        /// </remarks>
        /// <param name="query">The query.</param>
        /// <param name="loadedFieldNames">The fields to load in the result set.</param>
        /// <returns></returns>
        IBooleanOperation NativeQuery(string query, ISet<string> loadedFieldNames = null);
    }
}
