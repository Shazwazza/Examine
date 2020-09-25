using System.Collections.Generic;

namespace Examine
{
    public interface ISearchResults : IEnumerable<ISearchResult>
    {
        /// <summary>
        /// Returns the Total item count for the search regardless of skip/take/max count values
        /// </summary>
        long TotalItemCount { get; }

        /// <summary>
        /// Skips to a particular point in the search results.
        /// </summary>
        /// <remarks>
        /// This allows for lazy loading of the results paging. We don't go into Lucene until we have to.
        /// </remarks>
        /// <param name="skip">The number of items in the results to skip.</param>
        /// <returns>A collection of the search results</returns>
        IEnumerable<ISearchResult> Skip(int skip);
    }

    public interface ISearchResults2 : ISearchResults
    {
        /// <summary>
        /// Skips to a particular point in the search results, allows for efficient paging
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<ISearchResult> SkipTake(int skip, int? take = null);
    }
}
