using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search.Spell;
using System.Linq;
using Lucene.Net.Search.Suggest;
using Lucene.Net.Search.Suggest.Analyzing;
using Lucene.Net.Util;
using Examine.Suggest;
using static Lucene.Net.Search.Suggest.Lookup;
using System.Collections.Generic;

namespace Examine.Lucene.Suggest
{
    public class AnalyzingInfixSuggesterDefinition : LuceneSuggesterDefinition, ILookupExecutor
    {
        public AnalyzingInfixSuggesterDefinition(string name, string[] sourceFields = null, ISuggesterDirectoryFactory directoryFactory = null, Analyzer queryTimeAnalyzer = null)
            : base(name, sourceFields, directoryFactory,queryTimeAnalyzer)
        {
        }
        /// <inheritdoc/>
        public Lookup Lookup { get; internal set; }

        /// <inheritdoc/>
        public override ILookupExecutor BuildSuggester(FieldValueTypeCollection fieldValueTypeCollection, ReaderManager readerManager, bool rebuild)
            => BuildAnalyzingInfixSuggesterLookup(fieldValueTypeCollection, readerManager, rebuild);

        /// <inheritdoc/>
        public override ISuggestionResults ExecuteSuggester(string searchText, ISuggestionExecutionContext suggestionExecutionContext) => ExecuteAnalyzingInfixSuggester(searchText, suggestionExecutionContext);

        protected ILookupExecutor BuildAnalyzingInfixSuggesterLookup(FieldValueTypeCollection fieldValueTypeCollection, ReaderManager readerManager, bool rebuild)
        {
            string field = SourceFields.First();
            var fieldValue = GetFieldValueType(fieldValueTypeCollection, field);
            var indexTimeAnalyzer = fieldValue.Analyzer;


            AnalyzingInfixSuggester suggester = null;
            Analyzer queryTimeAnalyzer = QueryTimeAnalyzer;

            var luceneDictionary = SuggesterDirectoryFactory.CreateDirectory(Name.Replace(".", "_"), false);
            var luceneVersion = LuceneVersion.LUCENE_48;

            if (rebuild)
            {
                suggester = Lookup as AnalyzingInfixSuggester;
            }
            else if (queryTimeAnalyzer != null)
            {
                suggester = new AnalyzingInfixSuggester(luceneVersion, luceneDictionary, indexTimeAnalyzer, queryTimeAnalyzer, AnalyzingInfixSuggester.DEFAULT_MIN_PREFIX_CHARS);
            }
            else
            {
                suggester = new AnalyzingInfixSuggester(luceneVersion, luceneDictionary, indexTimeAnalyzer);
            }

            using (var readerReference = new IndexReaderReference(readerManager))
            {
                var lookupDictionary = new LuceneDictionary(readerReference.IndexReader, field);
                suggester.Build(lookupDictionary);
            }
            Lookup = suggester;
            return this;
        }

        /// <summary>
        /// Analyzing Infix Suggester Lookup
        /// </summary>
        private LuceneSuggestionResults ExecuteAnalyzingInfixSuggester(string searchText, ISuggestionExecutionContext suggestionExecutionContext)
        {
            AnalyzingInfixSuggester suggester = Lookup as AnalyzingInfixSuggester;

            var onlyMorePopular = false;
            if (suggestionExecutionContext.Options is LuceneSuggestionOptions luceneSuggestionOptions && luceneSuggestionOptions.SuggestionMode == LuceneSuggestionOptions.SuggestMode.SUGGEST_MORE_POPULAR)
            {
                onlyMorePopular = true;
            }

            IList<LookupResult> lookupResults;
            bool highlight = true;
            if (highlight)
            {
                lookupResults = suggester.DoLookup(searchText, null, suggestionExecutionContext.Options.Top, false, true);
            }
            else
            {
                lookupResults = suggester.DoLookup(searchText, onlyMorePopular, suggestionExecutionContext.Options.Top);
            }
            var results = lookupResults.Select(x => new SuggestionResult(x.Key, x.Value));
            LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
            return suggestionResults;
        }
    }
}
