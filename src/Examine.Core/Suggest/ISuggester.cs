namespace Examine.Suggest
{
    /// <summary>
    /// An interface representing an Examine Suggester.
    /// </summary>
    public interface ISuggester
    {
        /// <summary>
        /// Suggester Name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Suggest query terms for the given search Text
        /// </summary>
        /// <param name="searchText">Text to suggest on</param>
        /// <param name="sourceFieldName">Index field to suggest for</param>
        /// <param name="options">Options</param>
        /// <returns></returns>
        ISuggestionResults Suggest(string searchText, SuggestionOptions options = null);

        ISuggestionQuery CreateSuggestionQuery();

    }
}
