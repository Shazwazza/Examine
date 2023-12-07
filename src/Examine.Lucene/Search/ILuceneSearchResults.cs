namespace Examine.Lucene.Search
{
    /// <summary>
    /// Lucene.NET Search Results
    /// </summary>
    public interface ILuceneSearchResults : ISearchResults
    {
        /// <summary>
        /// Options for Searching After. Used for efficent deep paging.
        /// </summary>
        SearchAfterOptions? SearchAfter { get; }

        /// <summary>
        /// Returns the maximum score value encountered. Note that in case
        /// scores are not tracked, this returns <see cref="float.NaN"/>.
        /// </summary>
        float MaxScore { get; }
    }
}
