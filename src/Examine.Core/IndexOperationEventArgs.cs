using System;

namespace Examine
{
    /// <summary>
    /// Represents index operation event arguments
    /// </summary>
    public class IndexOperationEventArgs : EventArgs
    {
        /// <summary>
        /// The index of the event
        /// </summary>
        public IIndex Index { get; }

        /// <summary>
        /// The items indexed in operation
        /// </summary>
        public int ItemsIndexed { get; }

        /// <inheritdoc/>
        public IndexOperationEventArgs(IIndex index, int itemsIndexed)
        {
            Index = index;
            ItemsIndexed = itemsIndexed;
        }
    }
}
