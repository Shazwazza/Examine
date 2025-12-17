using System;
using System.Collections.Generic;

namespace Examine.Search
{
    public interface IFilter
    {
        /// <summary>
        /// Term must match
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        IBooleanFilterOperation TermFilter(FilterTerm term);

        /// <summary>
        /// Terms must match
        /// </summary>
        /// <param name="terms"></param>
        /// <returns></returns>
        IBooleanFilterOperation TermsFilter(IEnumerable<FilterTerm> terms);

        /// <summary>
        /// Term must match as prefix
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        IBooleanFilterOperation TermPrefixFilter(FilterTerm term);

        /// <summary>
        /// Document must have value for field
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        IBooleanFilterOperation FieldValueExistsFilter(string field);

        /// <summary>
        /// Document must not have value for field
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        IBooleanFilterOperation FieldValueNotExistsFilter(string field);

        /// <summary>
        /// Must match query
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp"></param>
        /// <returns></returns>
        IBooleanFilterOperation QueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);

        /// <summary>
        /// Matches items as defined by the IIndexFieldValueType used for the fields specified. 
        /// If a type is not defined for a field name, or the type does not implement IIndexRangeValueType for the types of min and max, nothing will be added
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="field"></param>
        /// <param name="minInclusive"></param>
        /// <param name="maxInclusive"></param>
        /// <returns></returns>
        IBooleanFilterOperation IntRangeFilter(string field, int? min, int? max, bool minInclusive, bool maxInclusive);


        /// <summary>
        /// Matches items as defined by the IIndexFieldValueType used for the fields specified. 
        /// If a type is not defined for a field name, or the type does not implement IIndexRangeValueType for the types of min and max, nothing will be added
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="field"></param>
        /// <param name="minInclusive"></param>
        /// <param name="maxInclusive"></param>
        /// <returns></returns>
        IBooleanFilterOperation LongRangeFilter(string field, long? min, long? max, bool minInclusive, bool maxInclusive);


        /// <summary>
        /// Matches items as defined by the IIndexFieldValueType used for the fields specified. 
        /// If a type is not defined for a field name, or the type does not implement IIndexRangeValueType for the types of min and max, nothing will be added
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="field"></param>
        /// <param name="minInclusive"></param>
        /// <param name="maxInclusive"></param>
        /// <returns></returns>
        IBooleanFilterOperation FloatRangeFilter(string field, float? min, float? max, bool minInclusive, bool maxInclusive);


        /// <summary>
        /// Matches items as defined by the IIndexFieldValueType used for the fields specified. 
        /// If a type is not defined for a field name, or the type does not implement IIndexRangeValueType for the types of min and max, nothing will be added
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="field"></param>
        /// <param name="minInclusive"></param>
        /// <param name="maxInclusive"></param>
        /// <returns></returns>
        IBooleanFilterOperation DoubleRangeFilter(string field, double? min, double? max, bool minInclusive, bool maxInclusive);

        /// <summary>
        /// Executes Spatial operation as a Filter on field and shape
        /// </summary>
        /// <param name="field">Index field name</param>
        /// <param name="shape">Shape</param>
        /// <returns></returns>
        IBooleanFilterOperation SpatialOperationFilter(string field, ExamineSpatialOperation spatialOperation, Func<ISpatialShapeFactory, ISpatialShape> shape);
    }
}
