using System.Collections.Generic;

namespace Examine.Suggest
{
    /// <summary>
    /// Search Suggestion Query
    /// </summary>
    public interface ISuggestionQuery
    {
        /// <summary>
        /// The source fields
        /// </summary>
        /// <param name="fieldNames">The field names for fields base suggestion on.</param>
        ISuggestionOrdering SourceFields(ISet<string> fieldNames);
    }
}
