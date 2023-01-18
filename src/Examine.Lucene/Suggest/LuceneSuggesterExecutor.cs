using System;
using System.Linq;
using Examine.Suggest;
using Lucene.Net.Index;
using Lucene.Net.Search.Spell;
using Lucene.Net.Search.Suggest;
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


        public LuceneSuggesterExecutor(string searchText, SuggestionOptions options, string sourceField, ISuggesterContext suggesterContext)
        {
            _searchText = searchText;
            _options = options;
            _sourceField = sourceField;
            _suggesterContext = suggesterContext;
        }

        /// <summary>
        /// Execute the Suggester
        /// </summary>
        /// <returns>Suggestion Results</returns>
        public ISuggestionResults Execute()
        {
            using (var readerReference = _suggesterContext.GetIndexReader())
            {
                string field = _sourceField;
                var fieldValue = _suggesterContext.GetFieldValueType(field);
                if (fieldValue != null && fieldValue.Lookup != null)
                {
                    return FieldDefinitionLookup(readerReference);
                }
                if (_options.SuggesterName == null)
                {
                    return _emptySuggestionResults;
                }
                if (_options.SuggesterName.Equals(ExamineLuceneSuggesterNames.AnalyzingSuggester, StringComparison.InvariantCultureIgnoreCase))
                {
                    return AnalyzingSuggester(readerReference);
                }
                if (_options.SuggesterName.Equals(ExamineLuceneSuggesterNames.FuzzySuggester, StringComparison.InvariantCultureIgnoreCase))
                {
                    return FuzzySuggester(readerReference);
                }
                if (_options.SuggesterName.StartsWith(ExamineLuceneSuggesterNames.DirectSpellChecker, StringComparison.InvariantCultureIgnoreCase))
                {
                    return DirectSpellChecker(readerReference);
                }
                return _emptySuggestionResults;
            }
        }

        private ISuggestionResults DirectSpellChecker(IIndexReaderReference readerReference)
        {
            string field = _sourceField;
            var fieldValue = _suggesterContext.GetFieldValueType(field);

            DirectSpellChecker spellchecker = new DirectSpellChecker();
            if (_options.SuggesterName.Equals(ExamineLuceneSuggesterNames.DirectSpellChecker_JaroWinklerDistance, StringComparison.InvariantCultureIgnoreCase)){
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

            var lookupResults = spellchecker.SuggestSimilar(new Term(field,_searchText), _options.Top, readerReference.IndexReader, SuggestMode.SUGGEST_WHEN_NOT_IN_INDEX);
            var results = lookupResults.Select(x => new SuggestionResult(x.String, x.Score, x.Freq));
            LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
            return suggestionResults;
        }

        private ISuggestionResults AnalyzingSuggester(IIndexReaderReference readerReference)
        {
            string field = _sourceField;
            var fieldValue = _suggesterContext.GetFieldValueType(field);
            LuceneDictionary luceneDictionary = new LuceneDictionary(readerReference.IndexReader, field);
            var analyzer = fieldValue.Analyzer;
            if(_options is LuceneSuggestionOptions luceneSuggestionOptions && luceneSuggestionOptions.Analyzer != null)
            {
                analyzer = luceneSuggestionOptions.Analyzer;
            }

            AnalyzingSuggester analyzingSuggester = new AnalyzingSuggester(analyzer);
            analyzingSuggester.Build(luceneDictionary);
            var lookupResults = analyzingSuggester.DoLookup(_searchText, false, _options.Top);
            var results = lookupResults.Select(x => new SuggestionResult(x.Key, x.Value));
            LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
            return suggestionResults;
        }

        private ISuggestionResults FuzzySuggester(IIndexReaderReference readerReference)
        {
            string field = _sourceField;
            var fieldValue = _suggesterContext.GetFieldValueType(field);
            LuceneDictionary luceneDictionary = new LuceneDictionary(readerReference.IndexReader, field);
            var analyzer = fieldValue.Analyzer;
            if (_options is LuceneSuggestionOptions luceneSuggestionOptions && luceneSuggestionOptions.Analyzer != null)
            {
                analyzer = luceneSuggestionOptions.Analyzer;
            }
            FuzzySuggester suggester = new FuzzySuggester(analyzer);
            suggester.Build(luceneDictionary);
            var lookupResults = suggester.DoLookup(_searchText, false, _options.Top);
            var results = lookupResults.Select(x => new SuggestionResult(x.Key, x.Value));
            LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
            return suggestionResults;
        }


        private ISuggestionResults FieldDefinitionLookup(IIndexReaderReference readerReference)
        {
            string field = _sourceField;
            var fieldValue = _suggesterContext.GetFieldValueType(field);
            LuceneDictionary luceneDictionary = new LuceneDictionary(readerReference.IndexReader, field);
            Lookup lookup = fieldValue.Lookup;
            lookup.Build(luceneDictionary);
            var lookupResults = lookup.DoLookup(_searchText, false, _options.Top);
            var results = lookupResults.Select(x => new SuggestionResult(x.Key, x.Value));
            LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
            return suggestionResults;
        }
            
    }
}
