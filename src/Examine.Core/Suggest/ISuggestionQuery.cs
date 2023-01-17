using System.Collections.Generic;

namespace Examine.Suggest
{
    /// <summary>
    /// Search Suggestion Query
    /// </summary>
    public interface ISuggestionQuery : ISuggestionExecutor
    {
        /// <summary>
        /// The source fields
        /// </summary>
        /// <param name="fieldNames">The field names for fields base suggestion on.</param>
        ISuggestionQuery SourceFields(ISet<string> fieldNames);
    }
}
