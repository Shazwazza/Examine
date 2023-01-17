using System.Collections.Generic;

namespace Examine.Suggest
{
    /// <summary>
    /// Search Suggestion Query
    /// </summary>
    public interface ISuggestionQuery : ISuggestionExecutor
    {
        /// <summary>
        /// The source field
        /// </summary>
        /// <param name="fieldName">The field name for field to base suggestion on.</param>
        ISuggestionQuery SourceField(string fieldName);
    }
}
