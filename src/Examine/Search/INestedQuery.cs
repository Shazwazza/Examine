using System.Collections.Generic;

namespace Examine.Search
{
    public interface INestedQuery
    {


        /// <summary>
        /// Query on the specified field for a struct value which will try to be auto converted with the correct query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        INestedBooleanOperation Field<T>(string fieldName, T fieldValue) where T : struct;

        /// <summary>
        /// Query on the specified field
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        INestedBooleanOperation Field(string fieldName, string fieldValue);

        /// <summary>
        /// Query on the specified field
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        INestedBooleanOperation Field(string fieldName, IExamineValue fieldValue);

        /// <summary>
        /// Queries multiple fields with each being an And boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        INestedBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query);

        /// <summary>
        /// Queries multiple fields with each being an And boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        INestedBooleanOperation GroupedAnd(IEnumerable<string> fields, params IExamineValue[] query);

        /// <summary>
        /// Queries multiple fields with each being an Or boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        INestedBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query);

        /// <summary>
        /// Queries multiple fields with each being an Or boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        INestedBooleanOperation GroupedOr(IEnumerable<string> fields, params IExamineValue[] query);

        /// <summary>
        /// Queries multiple fields with each being an Not boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        INestedBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query);

        /// <summary>
        /// Queries multiple fields with each being an Not boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        INestedBooleanOperation GroupedNot(IEnumerable<string> fields, params IExamineValue[] query);

        /// <summary>
        /// The index will determine the most appropriate way to search given the query and the fields provided
        /// </summary>
        /// <param name="query"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        INestedBooleanOperation ManagedQuery(string query, string[] fields = null);

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
        INestedBooleanOperation RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true) where T : struct;
    }
}