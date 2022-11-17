using System.Collections.Generic;

namespace Examine
{
    /// <summary>
    /// Represents search results from a query
    /// </summary>
    public interface ISearchResults : IEnumerable<ISearchResult>
    {
        /// <summary>
        /// Returns the Total item count for the search regardless of skip/take/max count values
        /// </summary>
        long TotalItemCount { get; }
    }
}
