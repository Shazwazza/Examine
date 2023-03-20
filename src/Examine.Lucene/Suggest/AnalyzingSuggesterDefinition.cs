using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search.Spell;
using System.Linq;
using Lucene.Net.Search.Suggest;
using Lucene.Net.Search.Suggest.Analyzing;
using Examine.Suggest;

namespace Examine.Lucene.Suggest
{
    public class AnalyzingSuggesterDefinition : LuceneSuggesterDefinition, ILookupExecutor
    {
        public AnalyzingSuggesterDefinition(string name, string[] sourceFields = null, ISuggesterDirectoryFactory directoryFactory = null)
            : base(name, sourceFields, directoryFactory)
        {
        }
        public Lookup Lookup { get; internal set; }

        public override ILookupExecutor BuildSuggester(FieldValueTypeCollection fieldValueTypeCollection, ReaderManager readerManager, bool rebuild)
            => BuildAnalyzingSuggesterLookup(fieldValueTypeCollection, readerManager, rebuild);
        public override ISuggestionResults ExecuteSuggester(string searchText, ISuggestionExecutionContext suggestionExecutionContext) => ExecuteAnalyzingSuggester(searchText, suggestionExecutionContext);

        protected ILookupExecutor BuildAnalyzingSuggesterLookup(FieldValueTypeCollection fieldValueTypeCollection, ReaderManager readerManager, bool rebuild)
        {
            string field = SourceFields.First();
            var fieldValue = GetFieldValueType(fieldValueTypeCollection, field);
            var indexTimeAnalyzer = fieldValue.Analyzer;

            AnalyzingSuggester suggester = null;
            Analyzer queryTimeAnalyzer = null;

            if (rebuild)
            {
                suggester = Lookup as AnalyzingSuggester;
            }
            else if (queryTimeAnalyzer != null)
            {
                suggester = new AnalyzingSuggester(indexTimeAnalyzer, queryTimeAnalyzer);
            }
            else
            {
                suggester = new AnalyzingSuggester(indexTimeAnalyzer);
            }

            using (var readerReference = new IndexReaderReference(readerManager))
            {
                var lookupDictionary = new LuceneDictionary(readerReference.IndexReader, field);
                suggester.Build(lookupDictionary);
            }
            Lookup = suggester;
            return this;
        }
        protected ISuggestionResults ExecuteAnalyzingSuggester(string searchText, ISuggestionExecutionContext suggestionExecutionContext)
        {
            AnalyzingSuggester analyzingSuggester = Lookup as AnalyzingSuggester;

            var onlyMorePopular = false;
            if (suggestionExecutionContext.Options is LuceneSuggestionOptions luceneSuggestionOptions
                && luceneSuggestionOptions.SuggestionMode == LuceneSuggestionOptions.SuggestMode.SUGGEST_MORE_POPULAR)
            {
                onlyMorePopular = true;
            }

            var lookupResults = analyzingSuggester.DoLookup(searchText, onlyMorePopular, suggestionExecutionContext.Options.Top);
            var results = lookupResults.Select(x => new SuggestionResult(x.Key, x.Value));
            LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
            return suggestionResults;
        }
    }
}

