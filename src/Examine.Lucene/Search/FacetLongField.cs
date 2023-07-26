using System.Collections.Generic;
using System.Linq;
using Examine.Search;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Range;
using Lucene.Net.Facet.SortedSet;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents a long facet field
    /// </summary>
    public readonly struct FacetLongField : IFacetField
    {
        /// <inheritdoc/>
        public string Field { get; }

        /// <summary>
        /// The long ranges
        /// </summary>
        public Examine.Search.Int64Range[] LongRanges { get; }

        /// <inheritdoc/>
        public string FacetField { get; }

        /// <inheritdoc/>
        public bool IsTaxonomyIndexed { get; }

        /// <inheritdoc/>
        public FacetLongField(string field, Examine.Search.Int64Range[] longRanges, string facetField, bool isTaxonomyIndexed = false)
        {
            Field = field;
            LongRanges = longRanges;
            FacetField = facetField;
            IsTaxonomyIndexed = isTaxonomyIndexed;
        }

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(IFacetExtractionContext facetExtractionContext)
        {
            var longFacetCounts = new Int64RangeFacetCounts(Field, facetExtractionContext.FacetsCollector, LongRanges.AsLuceneRange().ToArray());

            var longFacets = longFacetCounts.GetTopChildren(0, Field);

            if (longFacets == null)
            {
                yield break;
            }

            yield return new KeyValuePair<string, IFacetResult>(Field, new Examine.Search.FacetResult(longFacets.LabelValues.Select(labelValue => new FacetValue(labelValue.Label, labelValue.Value) as IFacetValue)));
        }
    }
}
