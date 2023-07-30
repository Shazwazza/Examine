using Examine.Suggest;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Lookup (Suggestion) Executor
    /// </summary>
    public interface ILookupExecutor
    {
        /// <summary>
        /// Executes a Suggester
        /// </summary>
        /// <param name="searchText">Search Text input</param>
        /// <param name="suggestionExecutionContext">Suggestion Context</param>
        /// <returns>Suggestion Results</returns>
        ISuggestionResults ExecuteSuggester(string searchText, ISuggestionExecutionContext suggestionExecutionContext);
    }
}
