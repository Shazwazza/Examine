using System;

namespace Examine
{
    public class IndexedNodeEventArgs : EventArgs
    {
        public IndexedNodeEventArgs(long nodeId)
        {
            NodeId = nodeId;
        }

        public long NodeId { get; private set; }
    }
}
