using System;

namespace Examine.Search
{

    /// <summary>
    /// Faceting operations
    /// </summary>
    public interface IFacetOperations : IQueryExecutor
    {
        /// <summary>
        /// Add a facet string to the current query, filtered by a single value or multiple values
        /// </summary>
        /// <param name="field"></param>
        /// <param name="facetConfiguration"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        IFacetOperations FacetString(string field, Action<IFacetQueryField>? facetConfiguration = null, params string[] values);

        /// <summary>
        /// Add a range facet to the current query
        /// </summary>
        IFacetOperations FacetDoubleRange(string field, params DoubleRange[] doubleRanges);

        /// <summary>
        /// Add a range facet to the current query
        /// </summary>
        IFacetOperations FacetFloatRange(string field, params FloatRange[] floatRanges);

        /// <summary>
        /// Add a range facet to the current query
        /// </summary>
        IFacetOperations FacetLongRange(string field, params Int64Range[] longRanges);
    }
}
