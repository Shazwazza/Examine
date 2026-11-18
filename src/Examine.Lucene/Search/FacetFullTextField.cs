using System;
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

        /// <inheritdoc/>
        public bool IsTaxonomyIndexed { get; }

        /// <inheritdoc/>
        public FacetFullTextField(string field, string[] values, string facetField, int maxCount = int.MaxValue, bool isTaxonomyIndexed = false)
        {
            Field = field;
            Values = values;
            FacetField = facetField;
            MaxCount = maxCount;
            IsTaxonomyIndexed = isTaxonomyIndexed;
        }

        // Lucene.Net's internal PriorityQueue maximum is int.MaxValue - 5 (ArrayUtil.MAX_ARRAY_LENGTH).
        // Use a conservative buffer so that any MaxCount value in that range triggers the two-pass probe.
        private const int LucenePriorityQueueBuffer = 256;
        private const int LuceneMaxTopChildren = int.MaxValue - LucenePriorityQueueBuffer;

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(IFacetExtractionContext facetExtractionContext)
        {
            var facetCounts = facetExtractionContext.GetFacetCounts(FacetField, IsTaxonomyIndexed);

            if (Values != null && Values.Length > 0)
            {
                var facetValues = new List<FacetValue>();
                foreach (var label in Values)
                {
                    var value = facetCounts.GetSpecificValue(Field, label);
                    facetValues.Add(new FacetValue(label, value));
                }
                facetValues.Sort((x, y) => x.Value.CompareTo(y.Value));
                var takeCount = Math.Min(MaxCount, facetValues.Count);
                var topFacetValues = new IFacetValue[takeCount];
                for (var i = 0; i < takeCount; i++)
                {
                    topFacetValues[i] = facetValues[i];
                }
                yield return new KeyValuePair<string, IFacetResult>(Field, new FacetResult(topFacetValues));
            }
            else
            {
                int topN;
                if (MaxCount >= LuceneMaxTopChildren)
                {
                    // Use a two-pass approach: probe with topN=1 to get the total ChildCount
                    // to avoid allocating a huge priority queue inside Lucene.Net
                    var probe = facetCounts.GetTopChildren(1, Field);
                    if (probe == null)
                    {
                        yield break;
                    }
                    topN = probe.ChildCount;
                    if (topN == 0)
                    {
                        yield break;
                    }
                }
                else
                {
                    topN = MaxCount;
                }

                var sortedFacets = facetCounts.GetTopChildren(topN, Field);

                if (sortedFacets == null)
                {
                    yield break;
                }

                yield return new KeyValuePair<string, IFacetResult>(Field, new FacetResult(sortedFacets.LabelValues.Select(labelValue => new FacetValue(labelValue.Label, labelValue.Value) as IFacetValue)));
            }
        }
    }
}
