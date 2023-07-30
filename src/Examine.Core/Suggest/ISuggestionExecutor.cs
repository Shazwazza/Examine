namespace Examine.Suggest
{
    /// <summary>
    /// Executes a Suggester
    /// </summary>
    public interface ISuggestionExecutor
    {
        /// <summary>
        /// Executes the query
        /// </summary>
        ISuggestionResults Execute(string searchText, SuggestionOptions? options = null);
    }
}
