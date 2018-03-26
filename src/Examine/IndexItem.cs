using System.Globalization;
using System.Xml.Linq;

namespace Examine
{
    /// <summary>
    /// Represents an item going into the index
    /// </summary>
    public struct IndexItem
    {
        private IndexItem(string id, string category)
        {
            Id = id;
            IndexCategory = category;
            ValueSet = default(ValueSet);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="valueSet"></param>
        public IndexItem(ValueSet valueSet)
        {
            ValueSet = valueSet;
            IndexCategory = ValueSet.Category;
            Id = ValueSet.Id.ToString(CultureInfo.InvariantCulture);
        }

        public static IndexItem ForCategory(string indexCategory)
        {
            return new IndexItem(null, indexCategory);
        }

        public static IndexItem ForId(string id)
        {
            return new IndexItem(id, null);
        }
        
        /// <summary>
        /// Exposes the underlying ValueSet
        /// </summary>
        public ValueSet ValueSet { get; }
        
        /// <summary>
        /// Gets or sets the type of the index.
        /// </summary>
        /// <value>
        /// The type of the index.
        /// </value>
        public string IndexCategory { get; private set; }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public string Id { get; private set; }
    }
}