using Lucene.Net.Index;
using Lucene.Net.Search.Spell;
using System.Linq;
using Examine.Suggest;
using System;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Lucene.NET NGramDistanceSuggester Definition
    /// </summary>
    public class NGramDistanceSuggesterDefinition : LuceneSuggesterDefinition, ILookupExecutor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Suggester Name</param>
        /// <param name="sourceFields">Source fields for suggestions</param>
        /// <param name="directoryFactory">Suggester Directory Factory</param>
        public NGramDistanceSuggesterDefinition(string name, string[]? sourceFields = null, ISuggesterDirectoryFactory? directoryFactory = null)
            : base(name, sourceFields, directoryFactory)
        {
        }

        /// <summary>
        /// Spellchecker
        /// </summary>
        public DirectSpellChecker? Spellchecker { get; set; }

        /// <inheritdoc/>
        public override ILookupExecutor BuildSuggester(FieldValueTypeCollection fieldValueTypeCollection, ReaderManager readerManager, bool rebuild)
            => BuildDirectSpellCheckerSuggester(fieldValueTypeCollection, readerManager, rebuild);

        /// <inheritdoc/>
        public override ISuggestionResults ExecuteSuggester(string searchText, ISuggestionExecutionContext suggestionExecutionContext) => ExecuteDirectSpellChecker(searchText, suggestionExecutionContext);

        /// <inheritdoc/>
        protected ILookupExecutor BuildDirectSpellCheckerSuggester(FieldValueTypeCollection fieldValueTypeCollection, ReaderManager readerManager, bool rebuild)
        {
            Spellchecker = new DirectSpellChecker();
            Spellchecker.Distance = new NGramDistance();
            return this;
        }

        private ISuggestionResults ExecuteDirectSpellChecker(string searchText, ISuggestionExecutionContext suggestionExecutionContext)
        {
            string field = SourceFields.First();
            var suggestMode = SuggestMode.SUGGEST_WHEN_NOT_IN_INDEX;
            if (suggestionExecutionContext.Options is LuceneSuggestionOptions luceneSuggestionOptions)
            {
                suggestMode = (SuggestMode)luceneSuggestionOptions.SuggestionMode;
            }

            if (Spellchecker is null)
            {
                throw new NullReferenceException("Spellchecker not set");
            }

            using (var readerReference = suggestionExecutionContext.GetIndexReader())
            {
                var lookupResults = Spellchecker.SuggestSimilar(new Term(field, searchText),
                                                                suggestionExecutionContext.Options.Top,
                                                                readerReference.IndexReader,
                                                                suggestMode);
                var results = lookupResults.Select(x => new SuggestionResult(x.String, x.Score, x.Freq));
                LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
                return suggestionResults;
            }
        }
    }
}
