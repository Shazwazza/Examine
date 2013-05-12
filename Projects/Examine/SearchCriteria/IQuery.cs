using System;
using System.Collections.Generic;
namespace Examine.SearchCriteria
{
    /// <summary>
    /// Defines the query methods for the fluent search API
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// Gets the boolean operation which this query method will be added as
        /// </summary>
        /// <value>The boolean operation.</value>
        BooleanOperation BooleanOperation { get; }

        /// <summary>
        /// Query on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        IBooleanOperation Id(int id);
        /// <summary>
        /// Query on the NodeName
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns></returns>
        IBooleanOperation NodeName(string nodeName);
        /// <summary>
        /// Query on the NodeName
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns></returns>
        IBooleanOperation NodeName(IExamineValue nodeName);
        /// <summary>
        /// Query on the NodeTypeAlias
        /// </summary>
        /// <param name="nodeTypeAlias">The node type alias.</param>
        /// <returns></returns>
        IBooleanOperation NodeTypeAlias(string nodeTypeAlias);
        /// <summary>
        /// Query on the NodeTypeAlias
        /// </summary>
        /// <param name="nodeTypeAlias">The node type alias.</param>
        /// <returns></returns>
        IBooleanOperation NodeTypeAlias(IExamineValue nodeTypeAlias);
        /// <summary>
        /// Query on the Parent ID
        /// </summary>
        /// <param name="id">The id of the parent.</param>
        /// <returns></returns>
        IBooleanOperation ParentId(int id);
        /// <summary>
        /// Query on the specified field
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        IBooleanOperation Field(string fieldName, string fieldValue);
        /// <summary>
        /// Query on the specified field
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        IBooleanOperation Field(string fieldName, IExamineValue fieldValue);
        /// <summary>
        /// Query on a specified field using a date range. Includes upper and lower bounds
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns></returns>
        IBooleanOperation Range(string fieldName, DateTime lower, DateTime upper);
        /// <summary>
        /// Query on a specified field using a date range using a default <see cref="DateResolution"/> of DateResolution.Millisecond
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <param name="includeLower">if set to <c>true</c> the lower.</param>
        /// <param name="includeUpper">if set to <c>true</c> the upper.</param>
        /// <returns></returns>
        IBooleanOperation Range(string fieldName, DateTime lower, DateTime upper, bool includeLower, bool includeUpper);
        /// <summary>
        /// Query on a specified field using a date range using the specified date resolution
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <param name="includeLower">if set to <c>true</c> [include lower].</param>
        /// <param name="includeUpper">if set to <c>true</c> [include upper].</param>
        /// <param name="resolution">The resolution of the date field.</param>
        /// <returns></returns>
        IBooleanOperation Range(string fieldName, DateTime lower, DateTime upper, bool includeLower, bool includeUpper, DateResolution resolution);
        /// <summary>
        /// Query on a specified field using an int range
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns></returns>
        IBooleanOperation Range(string fieldName, int lower, int upper);
        /// <summary>
        /// Query on a specified field using an int range. Includes upper and lower bounds
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <param name="includeLower">if set to <c>true</c> [include lower].</param>
        /// <param name="includeUpper">if set to <c>true</c> [include upper].</param>
        /// <returns></returns>
        IBooleanOperation Range(string fieldName, int lower, int upper, bool includeLower, bool includeUpper);
        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns></returns>
        IBooleanOperation Range(string fieldName, double lower, double upper);
        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <param name="includeLower">if set to <c>true</c> [include lower].</param>
        /// <param name="includeUpper">if set to <c>true</c> [include upper].</param>
        /// <returns></returns>
        IBooleanOperation Range(string fieldName, double lower, double upper, bool includeLower, bool includeUpper);
        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns></returns>
        IBooleanOperation Range(string fieldName, float lower, float upper);
        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <param name="includeLower">if set to <c>true</c> [include lower].</param>
        /// <param name="includeUpper">if set to <c>true</c> [include upper].</param>
        /// <returns></returns>
        IBooleanOperation Range(string fieldName, float lower, float upper, bool includeLower, bool includeUpper);
        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns></returns>
        IBooleanOperation Range(string fieldName, long lower, long upper);
        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <param name="includeLower">if set to <c>true</c> [include lower].</param>
        /// <param name="includeUpper">if set to <c>true</c> [include upper].</param>
        /// <returns></returns>
        IBooleanOperation Range(string fieldName, long lower, long upper, bool includeLower, bool includeUpper);
        /// <summary>
        /// Query on a specified field using a string range. Includes upper and lower bounds
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns></returns>
        IBooleanOperation Range(string fieldName, string lower, string upper);
        /// <summary>
        /// Query on a specified field using a string range
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <param name="includeLower">if set to <c>true</c> [include lower].</param>
        /// <param name="includeUpper">if set to <c>true</c> [include upper].</param>
        /// <returns></returns>
        IBooleanOperation Range(string fieldName, string lower, string upper, bool includeLower, bool includeUpper);
        
        /// <summary>
        /// Queries multiple fields with each being an And boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        IBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query);

        /// <summary>
        /// Queries multiple fields with each being an And boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        IBooleanOperation GroupedAnd(IEnumerable<string> fields, params IExamineValue[] query);

        /// <summary>
        /// Queries multiple fields with each being an Or boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        IBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query);

        /// <summary>
        /// Queries multiple fields with each being an Or boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        IBooleanOperation GroupedOr(IEnumerable<string> fields, params IExamineValue[] query);
        
        /// <summary>
        /// Queries multiple fields with each being an Not boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        IBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query);

        /// <summary>
        /// Queries multiple fields with each being an Not boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        IBooleanOperation GroupedNot(IEnumerable<string> fields, params IExamineValue[] query);

        /// <summary>
        /// Queries on multiple fields with their inclusions customly defined
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <param name="operations">The operations.</param>
        /// <returns></returns>
        IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params string[] query);

        /// <summary>
        /// Queries on multiple fields with their inclusions customly defined
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <param name="operations">The operations.</param>
        /// <returns></returns>
        IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params IExamineValue[] query);
        
        /// <summary>
        /// Orders the results by the specified fields
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns></returns>
        IBooleanOperation OrderBy(params string[] fieldNames);
        
        /// <summary>
        /// Orders the results by the specified fields in a descending order
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns></returns>
        IBooleanOperation OrderByDescending(params string[] fieldNames);


        /// <summary>
        /// Matches all items
        /// </summary>
        /// <returns></returns>
        IBooleanOperation All();

      
       
        /// <summary>
        /// Executes the query
        /// </summary>
        /// <returns></returns>
        ISearchResults Execute();
    }
}
