using Examine.Suggest;

namespace Examine.Lucene.Suggest
{
    public abstract class BaseLuceneSuggester : BaseSuggesterProvider
    {
        protected BaseLuceneSuggester(string name) : base(name)
        {
        }

        public abstract ISuggesterContext GetSuggesterContext();

        public override ISuggestionQuery CreateSuggestionQuery()
        {
            return CreateSuggestionQuery(new SuggestionOptions());
        }

        public virtual ISuggestionQuery CreateSuggestionQuery(SuggestionOptions options = null)
        {
            return new LuceneSuggestionQuery(GetSuggesterContext(), options);
        }
        public override ISuggestionResults Suggest(string searchText, SuggestionOptions options = null)
        {
            var suggestionExecutor = CreateSuggestionQuery();
            return suggestionExecutor.Execute(searchText, options);
        }
    }
}
