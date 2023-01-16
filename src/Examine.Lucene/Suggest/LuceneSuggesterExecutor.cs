using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Suggest;
using Lucene.Net.Search.Spell;
using Lucene.Net.Search.Suggest.Analyzing;

namespace Examine.Lucene.Suggest
{
    public class LuceneSuggesterExecutor
    {
        private string _searchText;
        private SuggestionOptions _options;
        private ISet<string> _sourceFields;
        private ISet<string> _fieldsToLoad;
        private ISuggesterContext _suggesterContext;

        public LuceneSuggesterExecutor(string searchText, SuggestionOptions options, ISet<string> sourceFields, ISet<string> fieldsToLoad, ISuggesterContext suggesterContext)
        {
            _searchText = searchText;
            _options = options;
            _sourceFields = sourceFields;
            _fieldsToLoad = fieldsToLoad;
            _suggesterContext = suggesterContext;
        }

        public ISuggestionResults Execute()
        {
            using (var readerReference = _suggesterContext.GetIndexReader())
            {
                string field = _sourceFields.First();
                var fieldValue = _suggesterContext.GetFieldValueType(field);
                
                LuceneDictionary luceneDictionary = new LuceneDictionary(readerReference.IndexReader, field);
                AnalyzingSuggester analyzingSuggester = new AnalyzingSuggester(fieldValue.Analyzer);
                analyzingSuggester.Build(luceneDictionary);
                var lookupResults = analyzingSuggester.DoLookup(_searchText, false, _options.Top);
                var results = lookupResults.Select(x => new SuggestionResult(x.Key, x.Value));
                LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
                return suggestionResults;
            }
        }
    }
}
