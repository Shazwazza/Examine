using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Facet.Range;

namespace Examine.Lucene
{
    /// <summary>
    /// Extensions related to faceting
    /// </summary>
    public static class FacetExtensions
    {
        /// <summary>
        /// Get the values for a particular facet in the results
        /// </summary>
        public static Examine.Search.IFacetResult GetFacet(this ISearchResults searchResults, string field)
        {
            if (!(searchResults is Examine.Search.IFacetResults facetResults))
            {
                throw new NotSupportedException("Result does not support facets");
            }

            facetResults.Facets.TryGetValue(field, out Examine.Search.IFacetResult facet);

            return facet;
        }

        /// <summary>
        /// Get all of the facets in the results
        /// </summary>
        public static IEnumerable<Examine.Search.IFacetResult> GetFacets(this ISearchResults searchResults)
        {
            if (!(searchResults is Examine.Search.IFacetResults facetResults))
            {
                throw new NotSupportedException("Result does not support facets");
            }

            return facetResults.Facets.Values;
        }

        /// <summary>
        /// Converts <see cref="Examine.Search.Int64Range"/> to the Lucene equivalent <see cref="Int64Range"/>
        /// </summary>
        /// <param name="ranges"></param>
        /// <returns></returns>
        internal static IEnumerable<Int64Range> AsLuceneRange(this IEnumerable<Examine.Search.Int64Range> ranges) => ranges.Select(range => range.AsLuceneRange());

        /// <summary>
        /// Converts a <see cref="Examine.Search.Int64Range"/> to the Lucene equivalent <see cref="Int64Range"/>
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static Int64Range AsLuceneRange(this Examine.Search.Int64Range range) => new Int64Range(range.Label, range.Min, range.MinInclusive, range.Max, range.MaxInclusive);

        /// <summary>
        /// Converts <see cref="Examine.Search.DoubleRange"/> to the Lucene equivalent <see cref="DoubleRange"/>
        /// </summary>
        /// <param name="ranges"></param>
        /// <returns></returns>
        internal static IEnumerable<DoubleRange> AsLuceneRange(this IEnumerable<Examine.Search.DoubleRange> ranges) => ranges.Select(range => range.AsLuceneRange());

        /// <summary>
        /// Converts a <see cref="Examine.Search.DoubleRange"/> to the Lucene equivalent <see cref="DoubleRange"/>
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static DoubleRange AsLuceneRange(this Examine.Search.DoubleRange range) => new DoubleRange(range.Label, range.Min, range.MinInclusive, range.Max, range.MaxInclusive);

        /// <summary>
        /// Converts <see cref="Examine.Search.FloatRange"/> to the Lucene equivalent <see cref="DoubleRange"/>
        /// </summary>
        /// <param name="ranges"></param>
        /// <returns></returns>
        internal static IEnumerable<DoubleRange> AsLuceneRange(this IEnumerable<Examine.Search.FloatRange> ranges) => ranges.Select(range => range.AsLuceneRange());

        /// <summary>
        /// Converts a <see cref="Examine.Search.DoubleRange"/> to the Lucene equivalent <see cref="DoubleRange"/>
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static DoubleRange AsLuceneRange(this Examine.Search.FloatRange range) => new DoubleRange(range.Label, range.Min, range.MinInclusive, range.Max, range.MaxInclusive);
    }
}
