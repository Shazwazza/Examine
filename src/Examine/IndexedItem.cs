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
        public static IndexedItem Empty { get; } = new IndexedItem(string.Empty, null);

        public bool IsEmpty()
        {
            return this == Empty;
        }

        public IndexedItem(string itemId, string category)
        {
            ItemId = itemId;
            Category = category;
        }

        public string ItemId { get; }
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
            return -2113648141 + EqualityComparer<string>.Default.GetHashCode(ItemId);
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
