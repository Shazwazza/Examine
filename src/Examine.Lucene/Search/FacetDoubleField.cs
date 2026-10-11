using Lucene.Net.Facet.Range;
using System.Collections.Generic;
using Examine.Search;
using System.Linq;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents a double facet field
    /// </summary>
    public readonly struct FacetDoubleField : IFacetField
    {
        /// <summary>
        /// The double ranges for the field
        /// </summary>
        public Examine.Search.DoubleRange[] DoubleRanges { get; }

        /// <inheritdoc/>
        public string Field { get; }

        /// <inheritdoc/>
        public string FacetField { get; }

        /// <inheritdoc/>
        public bool IsTaxonomyIndexed { get; }

        /// <inheritdoc/>
        public FacetDoubleField(string field, Examine.Search.DoubleRange[] doubleRanges, string facetField, bool isTaxonomyIndexed = false)
        {
            Field = field;
            DoubleRanges = doubleRanges;
            FacetField = facetField;
            IsTaxonomyIndexed = isTaxonomyIndexed;
        }

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(IFacetExtractionContext facetExtractionContext)
        {
            var doubleFacetCounts = new DoubleRangeFacetCounts(Field, facetExtractionContext.FacetsCollector, DoubleRanges.AsLuceneRange().ToArray());

            var doubleFacets = doubleFacetCounts.GetTopChildren(0, Field);

            if (doubleFacets == null)
            {
                yield break;
            }

            var labelValues = doubleFacets.LabelValues;
            var facetValues = new IFacetValue[labelValues.Length];
            for (var i = 0; i < labelValues.Length; i++)
            {
                var labelValue = labelValues[i];
                facetValues[i] = new FacetValue(labelValue.Label, labelValue.Value);
            }

            yield return new KeyValuePair<string, IFacetResult>(Field, new FacetResult(facetValues));
        }
    }
}
