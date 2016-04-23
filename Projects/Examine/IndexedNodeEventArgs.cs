using System;
using System.ComponentModel;

namespace Examine
{
    [Obsolete("Use the ItemIndexed event with ItemIndexedEventArgs instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
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
