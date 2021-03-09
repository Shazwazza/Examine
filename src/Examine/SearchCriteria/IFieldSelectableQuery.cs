using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.SearchCriteria
{
    public interface IFieldSelectableQuery : IQuery
    {
        /// <summary>
        /// Return only the specified fields. Use <see cref="SelectFields(Hashtable)"></see> when possible as internally a new Hashtable is created on each call/>
        /// </summary>
        /// <remarks>The Id field will also be retrieved as it is a required field.</remarks>
        /// <param name="fieldNames">The field names for fields to load</param>
        /// <returns></returns>
        IBooleanOperation SelectFields(params string[] fieldNames);

        /// <summary>
        /// Return only the specified fields
        /// </summary>
        /// <remarks>The Id field will also be retrieved as it is a required field.</remarks>
        /// <param name="fieldNames">The field names for fields to load. Key should be the field name, value should be null</param>
        /// <returns></returns>
        IBooleanOperation SelectFields(Hashtable fieldNames);

        /// <summary>
        /// Return only the specified field. Use <see cref="SelectFields(Hashtable)"></see> when possible as internally a new Hashtable is created on each call
        /// </summary>
        /// <remarks>The Id field will also be retrieved as it is a required field</remarks>
        /// <param name="fieldNames">The field name of the field to load</param>
        /// <returns></returns>
        IBooleanOperation SelectField(string fieldName);


        /// <summary>
        /// Return only the first field in the index
        /// </summary>
        /// <remarks>This should be the Id field as it should be first in the index</remarks>
        /// <returns></returns>
        IBooleanOperation SelectFirstFieldOnly();

        /// <summary>
        /// Return all fields in the index
        /// </summary>
        /// <returns></returns>
        IBooleanOperation SelectAllFields();
    }
}
