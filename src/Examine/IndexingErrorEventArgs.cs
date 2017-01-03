using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine
{
    public class IndexingErrorEventArgs : EventArgs, INodeEventArgs
    {

        public IndexingErrorEventArgs(string message, int nodeId, Exception innerException)
        {
            this.NodeId = nodeId;
            this.Message = message;
            this.InnerException = innerException;
        }

        public Exception InnerException { get; private set; }
        public string Message { get; private set; }

        #region INodeEventArgs Members

        public int NodeId { get; private set; }

        #endregion
    }
}
