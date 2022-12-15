using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Search
{
    /// <summary>
    /// Faceting operations
    /// </summary>
    public interface IFaceting
    {
        /// <summary>
        /// Add a facet string to the current query
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        IFacetQueryField WithFacet(string field);

        /// <summary>
        /// Add a facet string to the current query, filtered by a single value or multiple values
        /// </summary>
        /// <param name="field"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        IFacetQueryField WithFacet(string field, params string[] values);

        /// <summary>
        /// Add a range facet to the current query
        /// </summary>
        IFacetDoubleRangeQueryField WithFacet(string field, params DoubleRange[] doubleRanges);

        /// <summary>
        /// Add a range facet to the current query
        /// </summary>
        IFacetLongRangeQueryField WithFacet(string field, params Int64Range[] longRanges);
    }
}
