namespace Examine
{
    /// <summary>
    /// Represents an indexing operation (either add/remove)
    /// </summary>
    public class IndexOperation
    {
        /// <summary>
        /// Gets the Index item
        /// </summary>
        public IndexItem Item { get; set; }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        public IndexOperationType Operation { get; set; }
    }
}