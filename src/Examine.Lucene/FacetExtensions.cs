using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.Lucene.Search;
using Lucene.Net.Facet.Range;

namespace Examine.Lucene
{
    public static class FacetExtensions
    {
        /// <summary>
        /// Get the values for a particular facet in the results
        /// </summary>
        public static IFacetResult GetFacet(this ISearchResults searchResults, string field)
        {
            if (!(searchResults is IFacetResults facetResults))
            {
                throw new ArgumentException("Result does not support facets");
            }

            facetResults.Facets.TryGetValue(field, out IFacetResult facet);

            return facet;
        }

        /// <summary>
        /// Get all of the facets in the results
        /// </summary>
        public static IEnumerable<IFacetResult> GetFacets(this ISearchResults searchResults)
        {
            if (!(searchResults is IFacetResults facetResults))
            {
                throw new ArgumentException("Result does not support facets");
            }

            return facetResults.Facets.Values;
        }

        /// <summary>
        /// Converts <see cref="Examine.Search.Int64Range"/> to the Lucene equivalent <see cref="Int64Range"/>
        /// </summary>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public static IEnumerable<Int64Range> AsLuceneRange(this IEnumerable<Examine.Search.Int64Range> ranges) => ranges.Select(range => range.AsLuceneRange());

        /// <summary>
        /// Converts a <see cref="Examine.Search.Int64Range"/> to the Lucene equivalent <see cref="Int64Range"/>
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static Int64Range AsLuceneRange(this Examine.Search.Int64Range range) => new Int64Range(range.Label, range.Min, range.MinInclusive, range.Max, range.MaxInclusive);

        /// <summary>
        /// Converts <see cref="Examine.Search.DoubleRange"/> to the Lucene equivalent <see cref="DoubleRange"/>
        /// </summary>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public static IEnumerable<DoubleRange> AsLuceneRange(this IEnumerable<Examine.Search.DoubleRange> ranges) => ranges.Select(range => range.AsLuceneRange());

        /// <summary>
        /// Converts a <see cref="Examine.Search.DoubleRange"/> to the Lucene equivalent <see cref="DoubleRange"/>
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static DoubleRange AsLuceneRange(this Examine.Search.DoubleRange range) => new DoubleRange(range.Label, range.Min, range.MinInclusive, range.Max, range.MaxInclusive);
    }
}
