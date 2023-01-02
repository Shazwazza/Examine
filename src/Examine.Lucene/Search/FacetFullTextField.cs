using System.Collections.Generic;
using System.Linq;
using Examine.Search;
using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;

namespace Examine.Lucene.Search
{
    public class FacetFullTextField : IFacetField
    {
        public int MaxCount { get; internal set; }

        public string[] Values { get; }

        public string Field { get; }

        public string FacetField { get; }

        public FacetFullTextField(string field, string[] values, string facetField, int maxCount = 10)
        {
            Field = field;
            Values = values;
            FacetField = facetField;
            MaxCount = maxCount;
        }

        public void ExtractFacets(FacetsCollector facetsCollector, SortedSetDocValuesReaderState sortedSetReaderState, Dictionary<string, IFacetResult> facets)
        {
            var sortedFacetsCounts = new SortedSetDocValuesFacetCounts(sortedSetReaderState, facetsCollector);

            if (Values != null && Values.Length > 0)
            {
                var facetValues = new List<FacetValue>();
                foreach (var label in Values)
                {
                    var value = sortedFacetsCounts.GetSpecificValue(Field, label);
                    facetValues.Add(new FacetValue(label, value));
                }
                facets.Add(Field, new Examine.Search.FacetResult(facetValues.OrderBy(value => value.Value).Take(MaxCount).OfType<IFacetValue>()));
            }
            else
            {
                var sortedFacets = sortedFacetsCounts.GetTopChildren(MaxCount, Field);

                if (sortedFacets == null)
                {
                    return;
                }

                facets.Add(Field, new Examine.Search.FacetResult(sortedFacets.LabelValues.Select(labelValue => new FacetValue(labelValue.Label, labelValue.Value) as IFacetValue)));
            }
        }
    }
}
