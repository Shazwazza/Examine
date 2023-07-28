using System;
using System.Collections;
using System.Collections.Generic;
using Examine.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents the search results of a query
    /// </summary>
    public class LuceneSearchResults : ILuceneSearchResults, IFacetResults
    {
        private readonly IReadOnlyDictionary<string, IFacetResult> _noFacets = new Dictionary<string, IFacetResult>(0, StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Gets an empty search result
        /// </summary>
        public static LuceneSearchResults Empty { get; } = new LuceneSearchResults(Array.Empty<ISearchResult>(), 0, new Dictionary<string, IFacetResult>(), float.NaN, null);
        
        private readonly IReadOnlyCollection<ISearchResult> _results;

        /// <inheritdoc/>
        [Obsolete("To remove in Examine V5")]
        public LuceneSearchResults(IReadOnlyCollection<ISearchResult> results, int totalItemCount)
        {
            _results = results;
            TotalItemCount = totalItemCount;
            MaxScore = float.NaN;
            Facets = _noFacets;
        }

        /// <inheritdoc/>
        public LuceneSearchResults(IReadOnlyCollection<ISearchResult> results, int totalItemCount, IReadOnlyDictionary<string, IFacetResult> facets, float maxScore, SearchAfterOptions searchAfterOptions)
        {
            _results = results;
            TotalItemCount = totalItemCount;
            MaxScore = maxScore;
            SearchAfter = searchAfterOptions;
            Facets = facets;
        }

        /// <inheritdoc/>
        public long TotalItemCount { get; }

        /// <summary>
        /// Returns the maximum score value encountered. Note that in case
        /// scores are not tracked, this returns <see cref="float.NaN"/>.
        /// </summary>
        public float MaxScore { get; }

        /// <summary>
        /// Options for skipping documents after a previous search
        /// </summary>
        public SearchAfterOptions? SearchAfter { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, IFacetResult> Facets { get; }

        /// <inheritdoc/>
        public IEnumerator<ISearchResult> GetEnumerator() => _results.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
