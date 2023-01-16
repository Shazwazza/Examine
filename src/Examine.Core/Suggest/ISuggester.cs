using System.Collections.Generic;

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
        /// <param name="searchText"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        ISuggestionResults Suggest(string searchText, ISet<string> sourceFieldNames, SuggestionOptions options = null);

        ISuggestionQuery CreateSuggestionQuery();

    }
}
