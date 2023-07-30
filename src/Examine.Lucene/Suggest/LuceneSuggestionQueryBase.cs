using Examine.Suggest;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Base class for Lucene Suggesters
    /// </summary>
    public abstract class BaseLuceneSuggester : BaseSuggesterProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Suggester Name</param>
        protected BaseLuceneSuggester(string name) : base(name)
        {
        }

        /// <summary>
        /// Gets the Lucene Suggester Context
        /// </summary>
        /// <returns>Context</returns>
        public abstract ISuggesterContext GetSuggesterContext();

        /// <inheritdoc/>
        public override ISuggestionQuery CreateSuggestionQuery()
        {
            return CreateSuggestionQuery(new SuggestionOptions());
        }

        /// <summary>
        /// Create a Suggestion Query
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual ISuggestionQuery CreateSuggestionQuery(SuggestionOptions? options = null)
        {
            return new LuceneSuggestionQuery(GetSuggesterContext(), options);
        }

        /// <inheritdoc/>
        public override ISuggestionResults Suggest(string searchText, SuggestionOptions? options = null)
        {
            var suggestionExecutor = CreateSuggestionQuery();
            return suggestionExecutor.Execute(searchText, options);
        }
    }
}
