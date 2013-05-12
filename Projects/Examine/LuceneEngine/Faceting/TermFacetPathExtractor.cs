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
        public string Separator { get; set; }
        public Func<float, int, float> DistanceScorer { get; set; }

        public TermFacetPathExtractor(string fieldName, string separator = "/", Func<float, int, float> distanceScorer = null) : base(fieldName)
        {
            Separator = separator;
            DistanceScorer = DistanceScorer ?? ((level, distance) => level / distance);
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

                yield return new DocumentFacet
                {
                    DocumentId = docId,
                    Key = new FacetKey(fieldName, sb.ToString()),
                    Level = level/distance,
                    TermBased = false
                };
                --distance;
            }                      
        }
    }
}
