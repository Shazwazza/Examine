using System;
using System.Collections;
using System.Collections.Generic;
using Examine.Suggest;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Lucene Suggestion results
    /// </summary>
    public class LuceneSuggestionResults : ISuggestionResults
    {
        /// <summary>
        /// Empty Suggestion Results
        /// </summary>
        public static LuceneSuggestionResults Empty { get; } = new LuceneSuggestionResults(Array.Empty<ISuggestionResult>());

        private readonly IReadOnlyCollection<ISuggestionResult> _results;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="results">Suggestion Results</param>
        public LuceneSuggestionResults(IReadOnlyCollection<ISuggestionResult> results)
        {
            _results = results;
        }

        /// <inheritdoc/>
        public IEnumerator<ISuggestionResult> GetEnumerator() => _results.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
