using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Search
{
    // TODO: In v2.0 we can look at moving these to directly to IOrdering instead of a separate IFieldSelectableQuery with casting

    public interface IFieldSelectableOrdering : IOrdering
    {
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
