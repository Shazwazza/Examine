using System;
using System.Collections;
using System.Collections.Generic;
using Examine.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents the search results of a query
    /// </summary>
    public class LuceneSearchResults : ISearchResults, IFacetResults
    {
        /// <summary>
        /// Gets an empty search result
        /// </summary>
        public static LuceneSearchResults Empty { get; } = new LuceneSearchResults(Array.Empty<ISearchResult>(), 0, new Dictionary<string, IFacetResult>());

        private readonly IReadOnlyCollection<ISearchResult> _results;

        /// <inheritdoc/>
        public LuceneSearchResults(IReadOnlyCollection<ISearchResult> results, int totalItemCount, IReadOnlyDictionary<string, IFacetResult> facets)
        {
            _results = results;
            TotalItemCount = totalItemCount;
            Facets = facets;
        }

        /// <inheritdoc/>
        public long TotalItemCount { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, IFacetResult> Facets { get; }

        /// <inheritdoc/>
        public IEnumerator<ISearchResult> GetEnumerator() => _results.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
