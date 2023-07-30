using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search.Spell;
using System.Linq;
using Lucene.Net.Search.Suggest;
using Lucene.Net.Search.Suggest.Analyzing;
using Examine.Suggest;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Lucene.NET FuzzySuggester Definition
    /// </summary>
    public class FuzzySuggesterDefinition : LuceneSuggesterDefinition, ILookupExecutor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Suggester Name</param>
        /// <param name="sourceFields">Source fields for suggestions</param>
        /// <param name="directoryFactory">Suggester Directory Factory</param>
        /// <param name="queryTimeAnalyzer">Query Time Analyzer</param>
        public FuzzySuggesterDefinition(string name, string[]? sourceFields = null, ISuggesterDirectoryFactory? directoryFactory = null, Analyzer? queryTimeAnalyzer = null)
            : base(name, sourceFields, directoryFactory, queryTimeAnalyzer)
        {
        }

        /// <inheritdoc/>
        public Lookup? Lookup { get; internal set; }

        /// <inheritdoc/>
        public override ILookupExecutor BuildSuggester(FieldValueTypeCollection fieldValueTypeCollection, ReaderManager readerManager, bool rebuild)
            => BuildFuzzySuggesterLookup(fieldValueTypeCollection, readerManager, rebuild);

        /// <inheritdoc/>
        public override ISuggestionResults ExecuteSuggester(string searchText, ISuggestionExecutionContext suggestionExecutionContext) => ExecuteFuzzySuggester(searchText, suggestionExecutionContext);

        /// <inheritdoc/>
        protected ILookupExecutor BuildFuzzySuggesterLookup(FieldValueTypeCollection fieldValueTypeCollection, ReaderManager readerManager, bool rebuild)
        {
            string field = SourceFields.First();
            var fieldValue = GetFieldValueType(fieldValueTypeCollection, field);
            var indexTimeAnalyzer = fieldValue.Analyzer;


            FuzzySuggester? suggester = null;
            Analyzer? queryTimeAnalyzer = QueryTimeAnalyzer;

            if (rebuild)
            {
                suggester = Lookup as FuzzySuggester;
            }
            else if (queryTimeAnalyzer != null)
            {
                suggester = new FuzzySuggester(indexTimeAnalyzer, queryTimeAnalyzer);
            }
            else
            {
                suggester = new FuzzySuggester(indexTimeAnalyzer);
            }
            using (var readerReference = new IndexReaderReference(readerManager))
            {
                LuceneDictionary luceneDictionary = new LuceneDictionary(readerReference.IndexReader, field);
                suggester.Build(luceneDictionary);
            }
            Lookup = suggester;

            return this;
        }

        private ISuggestionResults ExecuteFuzzySuggester(string searchText, ISuggestionExecutionContext suggestionExecutionContext)
        {
            FuzzySuggester? suggester = Lookup as FuzzySuggester;

            var onlyMorePopular = false;
            if (suggestionExecutionContext.Options is LuceneSuggestionOptions luceneSuggestionOptions
                && luceneSuggestionOptions.SuggestionMode == LuceneSuggestionOptions.SuggestMode.SUGGEST_MORE_POPULAR)
            {
                onlyMorePopular = true;
            }
            var lookupResults = suggester.DoLookup(searchText, onlyMorePopular, suggestionExecutionContext.Options.Top);
            var results = lookupResults.Select(x => new SuggestionResult(x.Key, x.Value));
            LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
            return suggestionResults;
        }
    }
}
