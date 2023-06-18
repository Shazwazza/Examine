using System.Collections.Generic;
using System.Linq;
using Examine.Search;
using Lucene.Net.Facet;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents a full text facet field
    /// </summary>
    public class FacetFullTextField : IFacetField
    {
        /// <summary>
        /// Maximum number of terms to return
        /// </summary>
        public int MaxCount { get; internal set; }

        /// <summary>
        /// Filter values
        /// </summary>
        public string[] Values { get; }

        /// <inheritdoc/>
        public string Field { get; }

        /// <inheritdoc/>
        public string FacetField { get; }

        /// <summary>
        /// Path hierachy
        /// </summary>
        public string[]? Path { get; internal set; }

        /// <inheritdoc/>
        public bool IsTaxonomyIndexed { get; }

        /// <inheritdoc/>
        public FacetFullTextField(string field, string[] values, string facetField, int maxCount = 10, string[]? path = null, bool isTaxonomyIndexed = false)
        {
            Field = field;
            Values = values;
            FacetField = facetField;
            MaxCount = maxCount;
            Path = path;
            IsTaxonomyIndexed = isTaxonomyIndexed;
        }

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(IFacetExtractionContext facetExtractionContext)
        {
            Facets facetCounts = facetExtractionContext.GetFacetCounts(FacetField, false);

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
