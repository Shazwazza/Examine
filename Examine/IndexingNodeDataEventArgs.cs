using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Examine
{
    public class IndexingNodeDataEventArgs : IndexingNodeEventArgs
    {

        public IndexingNodeDataEventArgs(XElement node, int nodeId, Dictionary<string, string> fields, IndexType indexType)
            : base(nodeId, fields, indexType)
        {
            this.Node = node;
        }

        public XElement Node { get; private set; }
    }
}
