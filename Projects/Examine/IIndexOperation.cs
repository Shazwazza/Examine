namespace Examine
{
    public interface IIndexOperation
    {
        /// <summary>
        /// Gets the Index item
        /// </summary>
        IndexItem Item { get; }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        IndexOperationType Operation { get; }
    }
}