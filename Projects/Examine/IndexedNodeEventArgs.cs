using System;

namespace Examine
{
    public class IndexedNodeEventArgs : EventArgs
    {
        public IndexedNodeEventArgs(long nodeId)
        {
            NodeLongId = nodeId;
        }

        public long NodeLongId { get; private set; }

        public int NodeId
        {
            get { return (int)NodeLongId; }
            set { NodeLongId = value; }
        }
    }
}
