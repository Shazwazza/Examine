namespace Examine.Search
{
    /// <summary>
    /// Options for Searching After. Used for efficent deep paging.
    /// </summary>
    public class SearchAfter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="searchAfter">String representing the search after value</param>
        public SearchAfter(string searchAfter)
        {
            Value = searchAfter;
        }

        /// <summary>
        /// String representing the search after value
        /// </summary>
        public string Value { get; }
    }
}
