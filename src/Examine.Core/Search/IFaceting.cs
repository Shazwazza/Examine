using System;

namespace Examine.Search
{
    /// <summary>
    /// Faceting operations
    /// </summary>
    public interface IFaceting : IQueryExecutor
    {
        /// <summary>
        /// Add a facet string to the current query
        /// </summary>
        /// <param name="field"></param>
        /// <param name="facetConfiguration"></param>
        /// <returns></returns>
        IFaceting Facet(string field, Action<IFacetQueryField> facetConfiguration = null);

        /// <summary>
        /// Add a facet string to the current query, filtered by a single value or multiple values
        /// </summary>
        /// <param name="field"></param>
        /// <param name="facetConfiguration"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        IFaceting Facet(string field, Action<IFacetQueryField> facetConfiguration = null, params string[] values);

        /// <summary>
        /// Add a range facet to the current query
        /// </summary>
        IFaceting Facet(string field, params DoubleRange[] doubleRanges);

        /// <summary>
        /// Add a range facet to the current query
        /// </summary>
        IFaceting Facet(string field, params FloatRange[] floatRanges);

        /// <summary>
        /// Add a range facet to the current query
        /// </summary>
        IFaceting Facet(string field, params Int64Range[] longRanges);
    }
}
