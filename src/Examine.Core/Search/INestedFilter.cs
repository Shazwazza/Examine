using System;
using System.Collections.Generic;

namespace Examine.Search
{
    public interface INestedFilter
    {
        /// <summary>
        /// Chain filters
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        INestedBooleanFilterOperation NestedChainFilters(Action<IFilterChainStart> chain);

        /// <summary>
        /// Term must match
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        INestedBooleanFilterOperation NestedTermFilter(FilterTerm term);

        /// <summary>
        /// Terms must match
        /// </summary>
        /// <param name="terms"></param>
        /// <returns></returns>
        INestedBooleanFilterOperation NestedTermsFilter(IEnumerable<FilterTerm> terms);

        /// <summary>
        /// Term must match as prefix
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        INestedBooleanFilterOperation NestedTermPrefix(FilterTerm term);

        /// <summary>
        /// Document must have value for field
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        INestedBooleanFilterOperation NestedFieldValueExists(string field);

        /// <summary>
        /// Document must not have value for field
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        INestedBooleanFilterOperation NestedFieldValueNotExists(string field);

        /// <summary>
        /// Must match query
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp"></param>
        /// <returns></returns>
        INestedBooleanFilterOperation NestedQueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);

        /// <summary>
        /// Matches items as defined by the IIndexFieldValueType used for the fields specified. 
        /// If a type is not defined for a field name, or the type does not implement IIndexRangeValueType for the types of min and max, nothing will be added
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="field"></param>
        /// <param name="minInclusive"></param>
        /// <param name="maxInclusive"></param>
        /// <returns></returns>
        INestedBooleanFilterOperation NestedRangeFilter<T>(string field, T min, T max, bool minInclusive = true, bool maxInclusive = true) where T : struct;
    }
}
