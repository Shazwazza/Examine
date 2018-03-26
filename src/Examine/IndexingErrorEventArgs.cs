using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine
{
    public class IndexingErrorEventArgs : EventArgs
    {

        public IndexingErrorEventArgs(IIndexer indexer, string message, string itemId, Exception innerException)
        {
            Indexer = indexer;
            this.ItemId = itemId;
            this.Message = message;
            this.InnerException = innerException;
        }

        public Exception InnerException { get; }
        public string Message { get; }
        public IIndexer Indexer { get; }
        public string ItemId { get; }
    }
}
