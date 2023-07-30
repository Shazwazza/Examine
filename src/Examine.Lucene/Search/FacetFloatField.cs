using System.Collections.Generic;
using System.Linq;
using Examine.Search;
using Lucene.Net.Facet.Range;
using Lucene.Net.Queries.Function.ValueSources;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents a float facet field
    /// </summary>
    public readonly struct FacetFloatField : IFacetField
    {
        /// <summary>
        /// The float ranges for the field
        /// </summary>
        public FloatRange[] FloatRanges { get; }

        /// <inheritdoc/>
        public string Field { get; }

        /// <inheritdoc/>
        public string FacetField { get; }

        /// <inheritdoc/>
        public bool IsTaxonomyIndexed { get; }

        /// <inheritdoc/>
        public FacetFloatField(string field, FloatRange[] floatRanges, string facetField, bool isTaxonomyIndexed = false)
        {
            Field = field;
            FloatRanges = floatRanges;
            FacetField = facetField;
            IsTaxonomyIndexed = isTaxonomyIndexed;
        }

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(IFacetExtractionContext facetExtractionContext)
        {
            var floatFacetCounts = new DoubleRangeFacetCounts(Field, new SingleFieldSource(Field), facetExtractionContext.FacetsCollector, FloatRanges.AsLuceneRange().ToArray());

            var floatFacets = floatFacetCounts.GetTopChildren(0, Field);

            if (floatFacets == null)
            {
                yield break;
            }

            yield return new KeyValuePair<string, IFacetResult>(Field, new Examine.Search.FacetResult(floatFacets.LabelValues.Select(labelValue => new FacetValue(labelValue.Label, labelValue.Value) as IFacetValue)));
        }

    }
}
