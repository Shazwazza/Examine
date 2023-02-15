using System.Collections.Generic;
using System.Linq;
using Examine.Search;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Range;
using Lucene.Net.Facet.SortedSet;
using Lucene.Net.Queries.Function.ValueSources;

namespace Examine.Lucene.Search
{
    public readonly struct FacetFloatField : IFacetField
    {
        public FloatRange[] FloatRanges { get; }

        public string Field { get; }

        public string FacetField { get; }

        public FacetFloatField(string field, FloatRange[] floatRanges, string facetField)
        {
            Field = field;
            FloatRanges = floatRanges;
            FacetField = facetField;
        }

        public IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(FacetsCollector facetsCollector, SortedSetDocValuesReaderState sortedSetReaderState)
        {
            var floatFacetCounts = new DoubleRangeFacetCounts(Field, new SingleFieldSource(Field), facetsCollector, FloatRanges.AsLuceneRange().ToArray());

            var floatFacets = floatFacetCounts.GetTopChildren(0, Field);

            if (floatFacets == null)
            {
                yield break;
            }

            yield return new KeyValuePair<string, IFacetResult>(Field, new Examine.Search.FacetResult(floatFacets.LabelValues.Select(labelValue => new FacetValue(labelValue.Label, labelValue.Value) as IFacetValue)));
        }
    }
}
