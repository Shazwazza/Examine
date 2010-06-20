using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Examine
{

    /// <summary>
    /// Simple class to store the definition of an indexed node
    /// </summary>
    public class IndexedNode
    {
        public int NodeId { get; set; }
        public string Type { get; set; }
    }
}
