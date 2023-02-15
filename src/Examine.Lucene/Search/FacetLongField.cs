using System.Collections.Generic;
using System.Linq;
using Examine.Search;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Range;
using Lucene.Net.Facet.SortedSet;

namespace Examine.Lucene.Search
{
    public readonly struct FacetLongField : IFacetField
    {
        public string Field { get; }

        public Examine.Search.Int64Range[] LongRanges { get; }

        public string FacetField { get; }

        public FacetLongField(string field, Examine.Search.Int64Range[] longRanges, string facetField)
        {
            Field = field;
            LongRanges = longRanges;
            FacetField = facetField;
        }

        public IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(FacetsCollector facetsCollector, SortedSetDocValuesReaderState sortedSetReaderState)
        {
            var longFacetCounts = new Int64RangeFacetCounts(Field, facetsCollector, LongRanges.AsLuceneRange().ToArray());

            var longFacets = longFacetCounts.GetTopChildren(0, Field);

            if (longFacets == null)
            {
                yield break;
            }

            yield return new KeyValuePair<string, IFacetResult>(Field, new Examine.Search.FacetResult(longFacets.LabelValues.Select(labelValue => new FacetValue(labelValue.Label, labelValue.Value) as IFacetValue)));
        }
    }
}
