using System;
using System.Collections.Generic;

namespace Examine.Search
{
    /// <summary>
    /// Defines the query methods for the fluent search API
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// Passes a text string which is preformatted for the underlying search API. Examine will not modify this
        /// </summary>
        /// <remarks>
        /// This allows a developer to completely bypass and Examine logic and comprise their own query text which they are passing in.
        /// It means that if the search is too complex to achieve with the fluent API, or too dynamic to achieve with a static language
        /// the provider can still handle it.
        /// </remarks>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        IBooleanOperation NativeQuery(string query);

        /// <summary>
        /// Creates an inner group query
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp">The default operation is OR, generally a grouped query would have complex inner queries with an OR against another complex group query</param>
        /// <returns></returns>
        IBooleanOperation Group(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.Or);

        /// <summary>
        /// Query on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        IBooleanOperation Id(string id);

        /// <summary>
        /// Query on the specified field for a struct value which will try to be auto converted with the correct query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        IBooleanOperation Field<T>(string fieldName, T fieldValue) where T : struct;

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
        /// Matches all items
        /// </summary>
        /// <returns></returns>
        IOrdering All();

        /// <summary>
        /// The index will determine the most appropriate way to search given the query and the fields provided
        /// </summary>
        /// <param name="query"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        IBooleanOperation ManagedQuery(string query, string[]? fields = null);

        /// <summary>
        /// Matches items as defined by the IIndexFieldValueType used for the fields specified. 
        /// If a type is not defined for a field name, or the type does not implement IIndexRangeValueType for the types of min and max, nothing will be added
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="fields"></param>
        /// <param name="minInclusive"></param>
        /// <param name="maxInclusive"></param>
        /// <returns></returns>
        IBooleanOperation RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true) where T : struct;
    }
}
