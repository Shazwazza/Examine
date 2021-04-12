namespace Examine
{
    /// <summary>
    /// Represents an indexing operation (either add/remove)
    /// </summary>
    public struct IndexOperation
    {   
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public IndexOperation(ValueSet valueSet, IndexOperationType operation)
        {
            ValueSet = valueSet;
            Operation = operation;
        }

        /// <summary>
        /// Gets the Index item
        /// </summary>
        public ValueSet ValueSet { get; }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        public IndexOperationType Operation { get; }
    }
}