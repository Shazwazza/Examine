using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine
{
    /// <summary>
    /// Indexing error event arguments
    /// </summary>
    public class IndexingErrorEventArgs : EventArgs
    {
        /// <inheritdoc/>
        public IndexingErrorEventArgs(IIndex index, string message, string? itemId, Exception? exception)
        {
            Index = index;
            ItemId = itemId;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// The exception of the error
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// The message of the error
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The index where the error originated
        /// </summary>
        public IIndex Index { get; }

        /// <summary>
        /// The item id
        /// </summary>
        public string? ItemId { get; }
    }
}
