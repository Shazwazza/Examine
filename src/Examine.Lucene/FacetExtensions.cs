using System;
using System.Collections.Generic;
using Examine.Lucene.Search;

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
    }
}
