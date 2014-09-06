using System;
using System.Collections.Generic;
using System.Text;

namespace Examine.LuceneEngine.Faceting
{
    /// <summary>
    /// Like term TermFacetExtractor, but allows a field to contain hierarchical facets separated by a string
    /// </summary>
    public class TermFacetPathExtractor : TermFacetExtractor
    {
        public string Separator { get; private set; }
        public Func<float, int, float> DistanceScorer { get; private set; }

        public TermFacetPathExtractor(string fieldName, string separator = "/", Func<float, int, float> distanceScorer = null) : base(fieldName)
        {
            Separator = separator;
            DistanceScorer = distanceScorer ?? ((level, distance) => level / distance);
        }

        protected override IEnumerable<DocumentFacet> ExpandTerm(int docId, string fieldName, string termValue, float level)
        {
            var parts = termValue.Split(new [] {Separator}, StringSplitOptions.RemoveEmptyEntries);

            var distance = parts.Length;
            var sb = new StringBuilder();
            foreach (var p in parts)
            {
                if (sb.Length > 0) sb.Append(Separator);
                sb.Append(p);

                yield return new DocumentFacet(docId, false, new FacetKey(fieldName, sb.ToString()), level/distance);

                --distance;
            }                      
        }
    }
}
