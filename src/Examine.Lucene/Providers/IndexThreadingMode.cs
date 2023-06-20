namespace Examine.Lucene.Providers
{
    /// <summary>
    /// Represents the threading mode of indexing documents
    /// </summary>
    public enum IndexThreadingMode
    {
        /// <summary>
        /// The deafult, processes the index queue on a background thread
        /// </summary>
        Asynchronous,

        /// <summary>
        /// Optional, mostly for testing, processes the index queue on the same execution thread (blocking)
        /// </summary>
        Synchronous
    }


}

