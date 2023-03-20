using Lucene.Net.Facet.Range;
using System.Collections.Generic;
using Examine.Search;
using System.Linq;

namespace Examine.Lucene.Search
{
    public readonly struct FacetDoubleField : IFacetField
    {
        public Examine.Search.DoubleRange[] DoubleRanges { get; }

        public string Field { get; }

        public string FacetField { get; }
        public bool IsTaxonomyIndexed { get; }


        public FacetDoubleField(string field, Examine.Search.DoubleRange[] doubleRanges, string facetField, bool isTaxonomyIndexed = false)
        {
            Field = field;
            DoubleRanges = doubleRanges;
            FacetField = facetField;
            IsTaxonomyIndexed = isTaxonomyIndexed;
        }

        public IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(IFacetExtractionContext facetExtractionContext)
        {
            var doubleFacetCounts = new DoubleRangeFacetCounts(Field, facetExtractionContext.FacetsCollector, DoubleRanges.AsLuceneRange().ToArray());

            var doubleFacets = doubleFacetCounts.GetTopChildren(0, Field);

            if (doubleFacets == null)
            {
                yield break;
            }

            yield return new KeyValuePair<string, IFacetResult>(Field, new Examine.Search.FacetResult(doubleFacets.LabelValues.Select(labelValue => new FacetValue(labelValue.Label, labelValue.Value) as IFacetValue)));
        }
    }
}
