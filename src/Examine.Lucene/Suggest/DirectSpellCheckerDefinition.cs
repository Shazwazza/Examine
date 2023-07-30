using Lucene.Net.Index;
using Lucene.Net.Search.Spell;
using System.Linq;
using Examine.Suggest;
using System;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Lucene.NET DirectSpellChecker Suggester Defintion
    /// </summary>
    public class DirectSpellCheckerDefinition : LuceneSuggesterDefinition, ILookupExecutor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sourceFields"></param>
        /// <param name="directoryFactory"></param>
        public DirectSpellCheckerDefinition(string name, string[] sourceFields, ISuggesterDirectoryFactory? directoryFactory = null)
            : base(name, sourceFields, directoryFactory, default)
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

        /// <summary>
        /// Spell Checker
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
            return this;
        }

        private ISuggestionResults ExecuteDirectSpellChecker(string searchText, ISuggestionExecutionContext suggestionExecutionContext)
        {
            string field = (SourceFields?.FirstOrDefault()) ?? throw new InvalidOperationException("SourceFields can not be empty");
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
