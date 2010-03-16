using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace UmbracoExamine.Core
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
        /// <summary>
        /// Collection of node fields and their associated data
        /// </summary>
        public Dictionary<string, string> Values { get; private set; }

    }
}
