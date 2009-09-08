using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace UmbracoExamine
{
    public class IndexingNodeDataEventArgs : IndexingNodeEventArgs
    {

        public IndexingNodeDataEventArgs(XElement node, Dictionary<string, string> values, int nodeId)
            : base(nodeId)
        {
            this.Node = node;
            this.Values = values;
        }

        public XElement Node { get; private set; }
        public Dictionary<string, string> Values { get; private set; }

    }
}
