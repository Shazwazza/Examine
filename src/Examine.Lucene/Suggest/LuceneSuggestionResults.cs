using System;
using System.Collections;
using System.Collections.Generic;
using Examine.Suggest;

namespace Examine.Lucene.Suggest
{
    public class LuceneSuggestionResults : ISuggestionResults
    {
        public static LuceneSuggestionResults Empty { get; } = new LuceneSuggestionResults(Array.Empty<ISuggestionResult>());

        private readonly IReadOnlyCollection<ISuggestionResult> _results;

        public LuceneSuggestionResults(IReadOnlyCollection<ISuggestionResult> results)
        {
            _results = results;
        }


        public IEnumerator<ISuggestionResult> GetEnumerator() => _results.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
