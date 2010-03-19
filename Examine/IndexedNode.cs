using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace UmbracoExamine.Core
{

    /// <summary>
    /// Simple class to store the definition of an indexed node
    /// </summary>
    public class IndexedNode
    {
        public int NodeId { get; set; }
        public IndexType Type { get; set; }
    }
}
