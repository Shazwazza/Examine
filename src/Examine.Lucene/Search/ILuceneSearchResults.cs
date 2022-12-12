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
        SearchAfterOptions SearchAfter { get; }
    }
}
