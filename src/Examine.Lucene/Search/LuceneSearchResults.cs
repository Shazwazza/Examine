using System;
using System.Collections;
using System.Collections.Generic;

namespace Examine.Lucene.Search
{
    public class LuceneSearchResults : ILuceneSearchResults
    {
        public static LuceneSearchResults Empty { get; } = new LuceneSearchResults(Array.Empty<ISearchResult>(), 0, default);

        private readonly IReadOnlyCollection<ISearchResult> _results;

        public LuceneSearchResults(IReadOnlyCollection<ISearchResult> results, int totalItemCount, SearchAfterOptions searchAfterOptions)
        {
            _results = results;
            TotalItemCount = totalItemCount;
            SearchAfter = searchAfterOptions;
        }

        public long TotalItemCount { get; }

        public SearchAfterOptions SearchAfter { get; }

        public IEnumerator<ISearchResult> GetEnumerator() => _results.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
