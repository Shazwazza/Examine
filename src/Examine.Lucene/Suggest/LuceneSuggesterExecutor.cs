using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Suggest;
using Lucene.Net.Index;
using Lucene.Net.Search.Spell;
using Lucene.Net.Search.Suggest;
using Lucene.Net.Search.Suggest.Analyzing;

namespace Examine.Lucene.Suggest
{
    public class LuceneSuggesterExecutor
    {
        private string _searchText;
        private SuggestionOptions _options;
        private string _sourceField;
        private ISuggesterContext _suggesterContext;

        public LuceneSuggesterExecutor(string searchText, SuggestionOptions options, string sourceField, ISuggesterContext suggesterContext)
        {
            _searchText = searchText;
            _options = options;
            _sourceField = sourceField;
            _suggesterContext = suggesterContext;
        }

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
                    return FieldDefinitionLookup(readerReference);
                }
                if (_options.SuggesterName.Equals("AnalyzingSuggester", StringComparison.InvariantCultureIgnoreCase))
                {
                    return AnalyzingSuggester(readerReference);
                }
                if (_options.SuggesterName.Equals("DirectSpellChecker", StringComparison.InvariantCultureIgnoreCase))
                {
                    return DirectSpellChecker(readerReference);
                }
                return FieldDefinitionLookup(readerReference);
            }
        }

        private ISuggestionResults DirectSpellChecker(IIndexReaderReference readerReference)
        {
            string field = _sourceField;
            var fieldValue = _suggesterContext.GetFieldValueType(field);

            DirectSpellChecker spellchecker = new DirectSpellChecker();
            if (_options.SuggesterName.EndsWith("|JaroWinklerDistance")){
                spellchecker.Distance = new JaroWinklerDistance();
            }
            else if (_options.SuggesterName.EndsWith("|LevensteinDistance"))
            {
                spellchecker.Distance = new LevensteinDistance();
            }
            else if (_options.SuggesterName.EndsWith("|NGramDistance"))
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
            AnalyzingSuggester analyzingSuggester = new AnalyzingSuggester(fieldValue.Analyzer);
            analyzingSuggester.Build(luceneDictionary);
            var lookupResults = analyzingSuggester.DoLookup(_searchText, false, _options.Top);
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
