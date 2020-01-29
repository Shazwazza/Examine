using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine
{
    public class IndexingErrorEventArgs : EventArgs
    {

        public IndexingErrorEventArgs(IIndex index, string message, string itemId, Exception innerException)
        {
            Index = index;
            this.ItemId = itemId;
            this.Message = message;
            this.InnerException = innerException;
        }

        public Exception InnerException { get; }
        public string Message { get; }
        public IIndex Index { get; }
        public string ItemId { get; }
    }
}
