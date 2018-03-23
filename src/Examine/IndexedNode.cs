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
    internal struct IndexedNode : IEquatable<IndexedNode>
    {
        public IndexedNode(int nodeId, string category)
        {
            NodeId = nodeId;
            Category = category;
        }

        public int NodeId { get; }
        public string Category { get; }

        public bool Equals(IndexedNode other)
        {
            return NodeId == other.NodeId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is IndexedNode && Equals((IndexedNode)obj);
        }

        public override int GetHashCode()
        {
            return NodeId;
        }

        public static bool operator ==(IndexedNode left, IndexedNode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IndexedNode left, IndexedNode right)
        {
            return !left.Equals(right);
        }
    }
}
