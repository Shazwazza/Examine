using System;
using System.Linq;
using Examine.Suggest;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search.Spell;
using Lucene.Net.Search.Suggest.Analyzing;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Suggester Executor for a Lucene Index
    /// </summary>
    public class LuceneSuggesterExecutor
    {
        private readonly string _searchText;
        private readonly SuggestionOptions _options;
        private readonly string _sourceField;
        private readonly ISuggesterContext _suggesterContext;
        private readonly ISuggestionResults _emptySuggestionResults = new LuceneSuggestionResults(Array.Empty<ISuggestionResult>());

        public LuceneSuggesterExecutor(string searchText, SuggestionOptions options, ISuggesterContext suggesterContext)
        {
            _searchText = searchText;
            _options = options;
            _suggesterContext = suggesterContext;
        }

        /// <summary>
        /// Execute the Suggester
        /// </summary>
        /// <returns>Suggestion Results</returns>
        public ISuggestionResults Execute()
        {
            if (_options.SuggesterName == null)
            {
                return _emptySuggestionResults;
            }
            if (_options.SuggesterName.Equals(ExamineLuceneSuggesterNames.AnalyzingSuggester, StringComparison.InvariantCultureIgnoreCase))
            {
                return AnalyzingSuggester();
            }
            if (_options.SuggesterName.Equals(ExamineLuceneSuggesterNames.FuzzySuggester, StringComparison.InvariantCultureIgnoreCase))
            {
                return FuzzySuggester();
            }
            if (_options.SuggesterName.StartsWith(ExamineLuceneSuggesterNames.DirectSpellChecker, StringComparison.InvariantCultureIgnoreCase))
            {
                return DirectSpellChecker();
            }
            return _emptySuggestionResults;
        }

        private ISuggestionResults DirectSpellChecker()
        {
            using (var readerReference = _suggesterContext.GetIndexReader())
            {
                string field = _sourceField;
                var fieldValue = _suggesterContext.GetFieldValueType(field);

                DirectSpellChecker spellchecker = new DirectSpellChecker();
                if (_options.SuggesterName.Equals(ExamineLuceneSuggesterNames.DirectSpellChecker_JaroWinklerDistance, StringComparison.InvariantCultureIgnoreCase))
                {
                    spellchecker.Distance = new JaroWinklerDistance();
                }
                else if (_options.SuggesterName.Equals(ExamineLuceneSuggesterNames.DirectSpellChecker_LevensteinDistance, StringComparison.InvariantCultureIgnoreCase))
                {
                    spellchecker.Distance = new LevensteinDistance();
                }
                else if (_options.SuggesterName.Equals(ExamineLuceneSuggesterNames.DirectSpellChecker_NGramDistance, StringComparison.InvariantCultureIgnoreCase))
                {
                    spellchecker.Distance = new NGramDistance();
                }

                var suggestMode = SuggestMode.SUGGEST_WHEN_NOT_IN_INDEX;
                if (_options is LuceneSuggestionOptions luceneSuggestionOptions)
                {
                    suggestMode = (SuggestMode)luceneSuggestionOptions.SuggestionMode;
                }

                var lookupResults = spellchecker.SuggestSimilar(new Term(field, _searchText),
                                                                _options.Top,
                                                                readerReference.IndexReader,
                                                                suggestMode);
                var results = lookupResults.Select(x => new SuggestionResult(x.String, x.Score, x.Freq));
                LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
                return suggestionResults;
            }
        }

        private ISuggestionResults AnalyzingSuggester()
        {
            using (var readerReference = _suggesterContext.GetIndexReader())
            {
                string field = _sourceField;
                var fieldValue = _suggesterContext.GetFieldValueType(field);
                LuceneDictionary luceneDictionary = new LuceneDictionary(readerReference.IndexReader, field);
                var indexTimeAnalyzer = fieldValue.Analyzer;
                AnalyzingSuggester analyzingSuggester;
                var onlyMorePopular = false;
                Analyzer queryTimeAnalyzer = null;
                if (_options is LuceneSuggestionOptions luceneSuggestionOptions)
                {
                    if (queryTimeAnalyzer != null)
                    {
                        analyzingSuggester = new AnalyzingSuggester(indexTimeAnalyzer, queryTimeAnalyzer);
                    }
                    else
                    {
                        analyzingSuggester = new AnalyzingSuggester(indexTimeAnalyzer);
                    }
                    if (luceneSuggestionOptions.SuggestionMode == LuceneSuggestionOptions.SuggestMode.SUGGEST_MORE_POPULAR)
                    {
                        onlyMorePopular = true;
                    }
                }
                else
                {
                    analyzingSuggester = new AnalyzingSuggester(indexTimeAnalyzer);
                }

                analyzingSuggester.Build(luceneDictionary);
                var lookupResults = analyzingSuggester.DoLookup(_searchText, onlyMorePopular, _options.Top);
                var results = lookupResults.Select(x => new SuggestionResult(x.Key, x.Value));
                LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
                return suggestionResults;
            }
        }

        private ISuggestionResults FuzzySuggester()
        {
            using (var readerReference = _suggesterContext.GetIndexReader())
            {
                string field = _sourceField;
                var fieldValue = _suggesterContext.GetFieldValueType(field);
                LuceneDictionary luceneDictionary = new LuceneDictionary(readerReference.IndexReader, field);
                var indexTimeAnalyzer = fieldValue.Analyzer;

                FuzzySuggester suggester;
                var onlyMorePopular = false;
                Analyzer queryTimeAnalyzer = null;
                if (_options is LuceneSuggestionOptions luceneSuggestionOptions)
                {
                    if (queryTimeAnalyzer != null)
                    {
                        suggester = new FuzzySuggester(indexTimeAnalyzer, queryTimeAnalyzer);
                    }
                    else
                    {
                        suggester = new FuzzySuggester(indexTimeAnalyzer);
                    }
                    if (luceneSuggestionOptions.SuggestionMode == LuceneSuggestionOptions.SuggestMode.SUGGEST_MORE_POPULAR)
                    {
                        onlyMorePopular = true;
                    }
                }
                else
                {
                    suggester = new FuzzySuggester(indexTimeAnalyzer);
                }

                suggester.Build(luceneDictionary);
                var lookupResults = suggester.DoLookup(_searchText, onlyMorePopular, _options.Top);
                var results = lookupResults.Select(x => new SuggestionResult(x.Key, x.Value));
                LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
                return suggestionResults;
            }
        }

    }
}
