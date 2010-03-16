using System.ComponentModel;
using System.Collections.Generic;

namespace UmbracoExamine.Core
{
    public class IndexingNodeEventArgs : CancelEventArgs, INodeEventArgs
    {
        public IndexingNodeEventArgs(int nodeId, Dictionary<string, string> fields, IndexType indexType)
        {
            NodeId = nodeId;
            Fields = fields;
            IndexType = indexType;
        }

        public int NodeId { get; private set; }
        public Dictionary<string, string> Fields { get; private set; }
        public IndexType IndexType { get; private set; }
    }
}