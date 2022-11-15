using System;
using System.Collections;
using System.Collections.Generic;

namespace Examine.Lucene.Search
{
    public class LuceneSearchResults : ISearchResults, IFacetResults
    {
        public static LuceneSearchResults Empty { get; } = new LuceneSearchResults(Array.Empty<ISearchResult>(), 0, new Dictionary<string, IFacetResult>());

        private readonly IReadOnlyCollection<ISearchResult> _results;

        public LuceneSearchResults(IReadOnlyCollection<ISearchResult> results, int totalItemCount, IDictionary<string, IFacetResult> facets)
        {
            _results = results;
            TotalItemCount = totalItemCount;
            Facets = facets;
        }

        public long TotalItemCount { get; }

        public IDictionary<string, IFacetResult> Facets { get; }

        public IEnumerator<ISearchResult> GetEnumerator() => _results.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
