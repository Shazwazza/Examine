using Lucene.Net.Index;
using Lucene.Net.Search.Spell;
using System.Linq;
using Examine.Suggest;

namespace Examine.Lucene.Suggest
{
    public class JaroWinklerDistanceDefinition : LuceneSuggesterDefinition, ILookupExecutor
    {
        public JaroWinklerDistanceDefinition(string name, string[] sourceFields = null, ISuggesterDirectoryFactory directoryFactory = null)
            : base(name, sourceFields, directoryFactory)
        {
        }
        DirectSpellChecker Spellchecker { get; set; }

        public override ILookupExecutor BuildSuggester(FieldValueTypeCollection fieldValueTypeCollection, ReaderManager readerManager, bool rebuild)
            => BuildDirectSpellCheckerSuggester(fieldValueTypeCollection, readerManager, rebuild);
        public override ISuggestionResults ExecuteSuggester(string searchText, ISuggestionExecutionContext suggestionExecutionContext) => ExecuteDirectSpellChecker(searchText, suggestionExecutionContext);
        protected ILookupExecutor BuildDirectSpellCheckerSuggester(FieldValueTypeCollection fieldValueTypeCollection, ReaderManager readerManager, bool rebuild)
        {
            Spellchecker  = new DirectSpellChecker();
            Spellchecker.Distance = new JaroWinklerDistance();
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
