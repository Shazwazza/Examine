using System.Globalization;
using System.Xml.Linq;

namespace Examine
{
    /// <summary>
    /// Represents an item going into the index
    /// </summary>
    public class IndexItem
    {
        private IndexItem(){}

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
            return new IndexItem {IndexCategory = indexCategory};
        }

        public static IndexItem ForId(string id)
        {
            return new IndexItem { Id = id };
        }
        
        ///// <summary>
        ///// Initializes a new instance of the <see cref="IndexItem"/> class.
        ///// </summary>
        ///// <param name="data">The data.</param>
        ///// <param name="type">The type.</param>
        ///// <param name="id">The id.</param>
        //public IndexItem(XElement data, string type, string id)
        //{
        //    DataToIndex = data;
        //    IndexType = type;
        //    Id = id;
        //}

        /// <summary>
        /// Exposes the underlying ValueSet
        /// </summary>
        public ValueSet ValueSet { get; }

        ///// <summary>
        ///// Gets or sets the data.
        ///// </summary>
        ///// <value>
        ///// The data.
        ///// </value>
        //public XElement DataToIndex { get; private set; }

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