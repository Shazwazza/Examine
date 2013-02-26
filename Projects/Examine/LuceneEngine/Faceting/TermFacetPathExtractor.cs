using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Faceting
{
    /// <summary>
    /// Like term TermFacetExtractor, but allows a field to contain hierarchical facets separated by a string
    /// </summary>
    public class TermFacetPathExtractor : TermFacetExtractor
    {
        private readonly string _separator;

        public TermFacetPathExtractor(string fieldName, string separator = "/") : base(fieldName)
        {
            _separator = separator;
        }

        protected override IEnumerable<DocumentFacet> ExpandTerm(int docId, string fieldName, string termValue, float level)
        {
            var parts = termValue.Split(new [] {_separator}, StringSplitOptions.RemoveEmptyEntries);

            var distance = parts.Length;
            var sb = new StringBuilder();
            foreach (var p in parts)
            {
                if (sb.Length > 0) sb.Append(_separator);
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
