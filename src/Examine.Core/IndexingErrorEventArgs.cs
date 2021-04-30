using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine
{
    public class IndexingErrorEventArgs : EventArgs
    {

        public IndexingErrorEventArgs(IIndex index, string message, string itemId, Exception exception)
        {
            Index = index;
            ItemId = itemId;
            Message = message;
            Exception = exception;
        }

        public Exception Exception { get; }
        public string Message { get; }
        public IIndex Index { get; }
        public string ItemId { get; }
    }
}
