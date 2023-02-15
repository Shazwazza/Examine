using Lucene.Net.Facet.Range;
using Lucene.Net.Facet;
using System.Collections.Generic;
using Examine.Search;
using System.Linq;
using Lucene.Net.Facet.SortedSet;

namespace Examine.Lucene.Search
{
    public readonly struct FacetDoubleField : IFacetField
    {
        public Examine.Search.DoubleRange[] DoubleRanges { get; }

        public string Field { get; }

        public string FacetField { get; }

        public FacetDoubleField(string field, Examine.Search.DoubleRange[] doubleRanges, string facetField)
        {
            Field = field;
            DoubleRanges = doubleRanges;
            FacetField = facetField;
        }

        public IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(FacetsCollector facetsCollector, SortedSetDocValuesReaderState sortedSetReaderState)
        {
            var doubleFacetCounts = new DoubleRangeFacetCounts(Field, facetsCollector, DoubleRanges.AsLuceneRange().ToArray());

            var doubleFacets = doubleFacetCounts.GetTopChildren(0, Field);

            if (doubleFacets == null)
            {
                yield break;
            }

            yield return new KeyValuePair<string, IFacetResult>(Field, new Examine.Search.FacetResult(doubleFacets.LabelValues.Select(labelValue => new FacetValue(labelValue.Label, labelValue.Value) as IFacetValue)));
        }
    }
}
