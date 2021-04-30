using System;
using System.Collections;
using System.Collections.Generic;

namespace Examine.Lucene.Search
{
    public class LuceneSearchResults : ISearchResults
    {
        public static LuceneSearchResults Empty { get; } = new LuceneSearchResults(Array.Empty<ISearchResult>(), 0);

        private readonly IReadOnlyCollection<ISearchResult> _results;

        public LuceneSearchResults(IReadOnlyCollection<ISearchResult> results, int totalItemCount)
        {
            _results = results;
            TotalItemCount = totalItemCount;
        }

        public long TotalItemCount { get; }

        public IEnumerator<ISearchResult> GetEnumerator() => _results.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
