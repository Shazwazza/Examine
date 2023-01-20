using Examine.Suggest;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Lucene Search Suggestion Query
    /// </summary>
    public class LuceneSuggestionQuery : ISuggestionQuery, ISuggestionExecutor
    {
        private readonly ISuggesterContext _suggesterContext;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="suggesterContext">Lucene Suggestion Query Context</param>
        /// <param name="options">Query time suggester options</param>
        public LuceneSuggestionQuery(ISuggesterContext suggesterContext, SuggestionOptions options)
        {
            _suggesterContext = suggesterContext;
        }


        /// <inheritdoc/>
        public ISuggestionResults Execute(string searchText, SuggestionOptions options = null)
        {
            var executor = new LuceneSuggesterExecutor(searchText, options, _suggesterContext);
            var results = executor.Execute();
            return results;
        }
    }
}
