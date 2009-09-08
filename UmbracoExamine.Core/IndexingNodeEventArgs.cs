using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UmbracoExamine.Core
{
    public class IndexingNodeEventArgs : EventArgs 
    {

        public IndexingNodeEventArgs(int nodeId)
        {
            this.NodeId = nodeId;
        }

        public int NodeId { get; private set; }

    }
}
