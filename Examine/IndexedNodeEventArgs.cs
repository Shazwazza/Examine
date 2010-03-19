using System;

namespace UmbracoExamine.Core
{
    public class IndexedNodeEventArgs : EventArgs
    {
        public IndexedNodeEventArgs(int nodeId)
        {
            NodeId = nodeId;
        }

        public int NodeId { get; private set; }
    }
}
