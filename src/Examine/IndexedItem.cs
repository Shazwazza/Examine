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
    internal struct IndexedItem : IEquatable<IndexedItem>
    {
        private static readonly IndexedItem EmptyInstance = new IndexedItem(-1, null);
        public static IndexedItem Empty => EmptyInstance;

        public bool IsEmpty()
        {
            return this == EmptyInstance;
        }

        public IndexedItem(int itemId, string category)
        {
            ItemId = itemId;
            Category = category;
        }

        public int ItemId { get; }
        public string Category { get; }

        public bool Equals(IndexedItem other)
        {
            return ItemId == other.ItemId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is IndexedItem node && Equals(node);
        }

        public override int GetHashCode()
        {
            return ItemId;
        }

        public static bool operator ==(IndexedItem left, IndexedItem right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IndexedItem left, IndexedItem right)
        {
            return !left.Equals(right);
        }
    }
}
