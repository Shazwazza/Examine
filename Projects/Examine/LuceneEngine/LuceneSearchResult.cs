using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Examine.LuceneEngine.Faceting;
using Lucene.Net.Documents;

namespace Examine.LuceneEngine
{
    public class LuceneSearchResult : SearchResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuceneSearchResult"/> class.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <param name="longId"></param>
        /// <param name="facets">The facets.</param>
        /// <param name="facetCounts">The facet counts.</param>
        /// <param name="docId">The document identifier.</param>
        /// <param name="document">The document.</param>
        /// <param name="score"></param>
        public LuceneSearchResult(ILuceneSearchResults results, long longId, FacetLevel[] facets, FacetReferenceCount[] facetCounts, int docId, Document document, float score)
        {
            Score = score;
            LongId = longId;
            Results = results;
            Facets = facets;
            FacetCounts = facetCounts;
            DocId = docId;
            Document = document;
        }

        [ScriptIgnore]
        ILuceneSearchResults Results { get; set; }

        internal Document Document { get; set; }

        internal int DocId { get; set; }

        public FacetLevel[] Facets { get; private set; }

        /// <summary>
        /// How many times this document is used as a facet in the search results or facet count basis (SearchOptions.FacetReferenceCountBasis).
        /// </summary>
        public FacetReferenceCount[] FacetCounts { get; private set; }

        public string GetHighlight(string fieldName)
        {
            if (Results != null && Results.Highlighters != null)
            {
                List<Func<LuceneSearchResult, string>> hls;
                if (Results.Highlighters.TryGetValue(fieldName, out hls))
                {
                    return hls.Select(hl => hl(this)).FirstOrDefault(r => !string.IsNullOrWhiteSpace(r));
                }
            }


            return null;
        }
    }
}