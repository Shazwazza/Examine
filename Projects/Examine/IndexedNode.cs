using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Examine
{

    [Obsolete("This class is no longer used and will be removed in future versions")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IndexedNode
    {
        /// <summary>
        /// The node id
        /// </summary>
        public int NodeId { get; set; }

        /// <summary>
        /// The Node Type (this is not the index category)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Overridden to base equality on NodeId
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return ((IndexedNode)obj).NodeId == this.NodeId;
        }

        /// <summary>
        /// Overridden to base equality on NodeId
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.NodeId.GetHashCode();
        }
    }
}
