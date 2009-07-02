using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UmbracoExamine
{
    public class IndexingErrorEventArgs : IndexingNodeEventArgs
    {

        public IndexingErrorEventArgs(string message, int nodeId, Exception innerException)
            : base(nodeId)
        {
            this.Message = message;
            this.InnerException = innerException;
        }

        public Exception InnerException { get; private set; }
        public string Message { get; private set; }

    }
}
