using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search.Spell;
using System.Linq;
using Lucene.Net.Search.Suggest;
using Lucene.Net.Search.Suggest.Analyzing;
using Examine.Suggest;
using System;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Lucene.Net AnalyzingSuggester 
    /// </summary>
    public class AnalyzingSuggesterDefinition : LuceneSuggesterDefinition, ILookupExecutor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sourceFields"></param>
        /// <param name="directoryFactory"></param>
        /// <param name="queryTimeAnalyzer"></param>
        public AnalyzingSuggesterDefinition(string name, string[] sourceFields, ISuggesterDirectoryFactory? directoryFactory = null, Analyzer? queryTimeAnalyzer = null)
            : base(name, sourceFields, directoryFactory, queryTimeAnalyzer)
        {
            if (sourceFields is null)
            {
                throw new ArgumentNullException(nameof(sourceFields));
            }
            if (sourceFields.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceFields));
            }
        }

        /// <inheritdoc/>
        public Lookup? Lookup { get; internal set; }

        /// <inheritdoc/>
        public override ILookupExecutor BuildSuggester(FieldValueTypeCollection fieldValueTypeCollection, ReaderManager readerManager, bool rebuild)
            => BuildAnalyzingSuggesterLookup(fieldValueTypeCollection, readerManager, rebuild);

        /// <inheritdoc/>
        public override ISuggestionResults ExecuteSuggester(string searchText, ISuggestionExecutionContext suggestionExecutionContext) => ExecuteAnalyzingSuggester(searchText, suggestionExecutionContext);

        /// <inheritdoc/>
        protected ILookupExecutor BuildAnalyzingSuggesterLookup(FieldValueTypeCollection fieldValueTypeCollection, ReaderManager readerManager, bool rebuild)
        {
            string field = (SourceFields?.FirstOrDefault()) ?? throw new InvalidOperationException("SourceFields can not be empty");
            var fieldValue = GetFieldValueType(fieldValueTypeCollection, field);
            var indexTimeAnalyzer = fieldValue.Analyzer;

            AnalyzingSuggester? suggester = null;
            Analyzer? queryTimeAnalyzer = QueryTimeAnalyzer;

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

            if (suggester is null)
            {
                throw new NullReferenceException("Lookup or Analyzer not set");
            }

            using (var readerReference = new IndexReaderReference(readerManager))
            {
                var lookupDictionary = new LuceneDictionary(readerReference.IndexReader, field);
                suggester.Build(lookupDictionary);
            }

            Lookup = suggester;
            return this;
        }

        /// <inheritdoc/>
        protected ISuggestionResults ExecuteAnalyzingSuggester(string searchText, ISuggestionExecutionContext suggestionExecutionContext)
        {
            AnalyzingSuggester? analyzingSuggester = Lookup as AnalyzingSuggester ?? throw new InvalidCastException("Lookup is not AnalyzingSuggester");

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

