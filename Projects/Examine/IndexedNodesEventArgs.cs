using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Examine
{
    public class IndexedNodesEventArgs : EventArgs
    {

        public IndexedNodesEventArgs(IIndexCriteria indexData, IEnumerable<IndexedNode> nodes)
        {
            this.IndexData = indexData;
            this.Nodes = nodes;
        }

        public IIndexCriteria IndexData { get; private set; }
        public IEnumerable<IndexedNode> Nodes { get; private set; }

    }
}