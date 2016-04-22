using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine
{
    public class IndexingErrorEventArgs : EventArgs
    {

        public IndexingErrorEventArgs(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }

        public string Message { get; private set; }
        public Exception Exception { get; private set; }
        
    }
}
