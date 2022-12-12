using Examine.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Lucene.NET specific query options
    /// </summary>
    public class LuceneQueryOptions : QueryOptions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="skip">Number of result documents to skip.</param>
        /// <param name="take">Optional number of result documents to take.</param>
        /// <param name="searchAfter">Optionally skip to results after the results from the previous search execution. Used for efficent deep paging.</param>
        public LuceneQueryOptions(int skip, int? take = null, SearchAfterOptions searchAfter = null)
            : base(skip, take)
        {
            SearchAfter = searchAfter;
        }

        /// <summary>
        /// Options for Searching After. Used for efficent deep paging.
        /// </summary>
        public SearchAfterOptions SearchAfter { get; }
    }
}
