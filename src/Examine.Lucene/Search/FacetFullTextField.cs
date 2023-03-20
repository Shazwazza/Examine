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
        public bool IsTaxonomyIndexed { get; }
        public string[] Values { get; }

        public string Field { get; }

        public string FacetField { get; }

        public string[] Path { get; internal set; }

        public bool IsTaxonomyIndexed { get; }

        public FacetFullTextField(string field, string[] values, string facetField, int maxCount = 10, string[] path = null, bool isTaxonomyIndexed = false)
        {
            Field = field;
            Values = values;
            FacetField = facetField;
            MaxCount = maxCount;
            Path = path;
            IsTaxonomyIndexed = isTaxonomyIndexed;
        }

        public IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(IFacetExtractionContext facetExtractionContext)
        {
            Facets facetCounts = facetExtractionContext.GetFacetCounts(FacetField, IsTaxonomyIndexed);

            if (Values != null && Values.Length > 0)
            {
                var facetValues = new List<FacetValue>();
                foreach (var label in Values)
                {
                    var value = facetCounts.GetSpecificValue(Field, label);
                    facetValues.Add(new FacetValue(label, value));
                }
                yield return new KeyValuePair<string, IFacetResult>(Field, new Examine.Search.FacetResult(facetValues.OrderBy(value => value.Value).Take(MaxCount).OfType<IFacetValue>()));
            }
            else
            {
                var sortedFacets = facetCounts.GetTopChildren(MaxCount, Field);

                if (sortedFacets == null)
                {
                    yield break;
                }

                yield return new KeyValuePair<string, IFacetResult>(Field, new Examine.Search.FacetResult(sortedFacets.LabelValues.Select(labelValue => new FacetValue(labelValue.Label, labelValue.Value) as IFacetValue)));
            }
        }
    }
}
