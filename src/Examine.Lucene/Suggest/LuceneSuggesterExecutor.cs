using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Suggest;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search.Spell;
using Lucene.Net.Search.Suggest;
using Lucene.Net.Search.Suggest.Analyzing;
using Lucene.Net.Store;
using Lucene.Net.Util;
using static Lucene.Net.Search.Suggest.Lookup;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Suggester Executor for a Lucene Index
    /// </summary>
    public class LuceneSuggesterExecutor
    {
        private readonly string _searchText;
        private readonly SuggestionOptions _options;
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

            var suggesters = _suggesterContext.GetSuggesterDefinitions();
            var suggester = suggesters.FirstOrDefault(x => x.Name == _options.SuggesterName);
            if (suggester.Name == null || suggester.SuggesterMode == null || suggester.SourceFields == null)
            {
                return _emptySuggestionResults;
            }

            if (suggester.SuggesterMode.Equals(ExamineLuceneSuggesterNames.AnalyzingSuggester, StringComparison.InvariantCultureIgnoreCase))
            {
                return ExecuteAnalyzingSuggester(suggester);
            }
            if (suggester.SuggesterMode.Equals(ExamineLuceneSuggesterNames.FuzzySuggester, StringComparison.InvariantCultureIgnoreCase))
            {
                return ExecuteFuzzySuggester(suggester);
            }
            if (suggester.SuggesterMode.StartsWith(ExamineLuceneSuggesterNames.DirectSpellChecker, StringComparison.InvariantCultureIgnoreCase))
            {
                return ExecuteDirectSpellChecker(suggester);
            }
            if (suggester.SuggesterMode.Equals(ExamineLuceneSuggesterNames.AnalyzingInfixSuggester, StringComparison.InvariantCultureIgnoreCase)
                && suggester is LuceneSuggesterDefinition luceneSuggesterDefinition
                )
            {
                return ExecuteAnalyzingInfixSuggester(luceneSuggesterDefinition);
            }

            return _emptySuggestionResults;
        }

        private ISuggestionResults ExecuteDirectSpellChecker(SuggesterDefinition suggesterDefinition)
        {
            string field = suggesterDefinition.SourceFields.First();
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
            using (var readerReference = _suggesterContext.GetIndexReader())
            {
                var lookupResults = spellchecker.SuggestSimilar(new Term(field, _searchText),
                                                                _options.Top,
                                                                readerReference.IndexReader,
                                                                suggestMode);
                var results = lookupResults.Select(x => new SuggestionResult(x.String, x.Score, x.Freq));
                LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
                return suggestionResults;
            }
        }

        private ISuggestionResults ExecuteAnalyzingSuggester(SuggesterDefinition suggesterDefinition)
        {
            string field = suggesterDefinition.SourceFields.First();
            var fieldValue = _suggesterContext.GetFieldValueType(field);
            var indexTimeAnalyzer = fieldValue.Analyzer;
            AnalyzingSuggester analyzingSuggester;
            Analyzer queryTimeAnalyzer = null;
            if (queryTimeAnalyzer != null)
            {
                analyzingSuggester = new AnalyzingSuggester(indexTimeAnalyzer, queryTimeAnalyzer);
            }
            else
            {
                analyzingSuggester = new AnalyzingSuggester(indexTimeAnalyzer);
            }

            using (var readerReference = _suggesterContext.GetIndexReader())
            {
                LuceneDictionary luceneDictionary = new LuceneDictionary(readerReference.IndexReader, field);
                analyzingSuggester.Build(luceneDictionary);
            }

            var onlyMorePopular = false;
            if (_options is LuceneSuggestionOptions luceneSuggestionOptions
                && luceneSuggestionOptions.SuggestionMode == LuceneSuggestionOptions.SuggestMode.SUGGEST_MORE_POPULAR)
            {
                onlyMorePopular = true;
            }

            var lookupResults = analyzingSuggester.DoLookup(_searchText, onlyMorePopular, _options.Top);
            var results = lookupResults.Select(x => new SuggestionResult(x.Key, x.Value));
            LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
            return suggestionResults;
        }

        private ISuggestionResults ExecuteFuzzySuggester(SuggesterDefinition suggesterDefinition)
        {
            string field = suggesterDefinition.SourceFields.First();
            var fieldValue = _suggesterContext.GetFieldValueType(field);
            var indexTimeAnalyzer = fieldValue.Analyzer;

            FuzzySuggester suggester;
            Analyzer queryTimeAnalyzer = null;
            if (queryTimeAnalyzer != null)
            {
                suggester = new FuzzySuggester(indexTimeAnalyzer, queryTimeAnalyzer);
            }
            else
            {
                suggester = new FuzzySuggester(indexTimeAnalyzer);
            }
            using (var readerReference = _suggesterContext.GetIndexReader())
            {
                LuceneDictionary luceneDictionary = new LuceneDictionary(readerReference.IndexReader, field);
                suggester.Build(luceneDictionary);

            }
            var onlyMorePopular = false;
            if (_options is LuceneSuggestionOptions luceneSuggestionOptions
                && luceneSuggestionOptions.SuggestionMode == LuceneSuggestionOptions.SuggestMode.SUGGEST_MORE_POPULAR)
            {
                onlyMorePopular = true;
            }
            var lookupResults = suggester.DoLookup(_searchText, onlyMorePopular, _options.Top);
            var results = lookupResults.Select(x => new SuggestionResult(x.Key, x.Value));
            LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
            return suggestionResults;
        }

        /// <summary>
        /// Analyzing Infix Suggester Lookup
        /// </summary>
        private LuceneSuggestionResults ExecuteAnalyzingInfixSuggester(LuceneSuggesterDefinition suggesterDefinition, bool highlight = true)
        {
            string field = suggesterDefinition.SourceFields.First();
            var fieldValue = _suggesterContext.GetFieldValueType(field);
            var indexTimeAnalyzer = fieldValue.Analyzer;
            AnalyzingInfixSuggester suggester = null;
            Analyzer queryTimeAnalyzer = null;

            LuceneDirectory luceneDictionary = suggesterDefinition.SuggesterDirectoryFactory.CreateDirectory(suggesterDefinition.Name.Replace(".", "_"), false);

            try
            {
                using (var readerReference = _suggesterContext.GetIndexReader())
                {
                    var onlyMorePopular = false;
                    if (queryTimeAnalyzer != null)
                    {
                        suggester = new AnalyzingInfixSuggester(_suggesterContext.GetLuceneVersion(), luceneDictionary, indexTimeAnalyzer, queryTimeAnalyzer, AnalyzingInfixSuggester.DEFAULT_MIN_PREFIX_CHARS);
                    }
                    else
                    {
                        suggester = new AnalyzingInfixSuggester(_suggesterContext.GetLuceneVersion(), luceneDictionary, indexTimeAnalyzer);
                    }

                    var lookupDictionary = new LuceneDictionary(readerReference.IndexReader, field);
                    suggester.Build(lookupDictionary);

                    if (_options is LuceneSuggestionOptions luceneSuggestionOptions && luceneSuggestionOptions.SuggestionMode == LuceneSuggestionOptions.SuggestMode.SUGGEST_MORE_POPULAR)
                    {
                        onlyMorePopular = true;
                    }

                    IList<LookupResult> lookupResults;
                    if (highlight)
                    {
                        lookupResults = suggester.DoLookup(_searchText, null, _options.Top, false, true);
                    }
                    else
                    {
                        lookupResults = suggester.DoLookup(_searchText, onlyMorePopular, _options.Top);
                    }
                    var results = lookupResults.Select(x => new SuggestionResult(x.Key, x.Value));
                    LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
                    return suggestionResults;
                }
            }
            finally
            {
                suggester?.Dispose();
            }
        }
    }
}
