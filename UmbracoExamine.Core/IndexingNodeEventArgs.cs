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
            //TermRemoved = null;
            NodeId = nodeId;
        }

        //public IndexingNodeEventArgs(KeyValuePair<string, string> termRemoved)
        //{
        //    this.TermRemoved = termRemoved;
        //    NodeId = null;
        //}

        public int NodeId { get; private set; }
        //public KeyValuePair<string, string>? TermRemoved { get; private set; }

    }
}