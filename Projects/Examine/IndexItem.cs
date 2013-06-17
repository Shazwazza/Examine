using System;
using System.Xml.Linq;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine;

namespace Examine
{
    /// <summary>
    /// Represents an item going into the index
    /// </summary>
    public class IndexItem
    {
        private XElement _dataToIndex;
        private string _indexType;
        private string _id;
        public ValueSet ValueSet { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexItem"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="type">The type.</param>
        /// <param name="id">The id.</param>
        [Obsolete("Use ValueSets instead")]
        public IndexItem(XElement data, string type, string id)
           : this(data.ToValueSet(type, long.Parse(id)))
        {
            DataToIndex = data;
            IndexType = type;
            Id = id;            
        }

        void InitializeLegacyFields()
        {
            if (_dataToIndex == null && ValueSet != null)
            {
                _dataToIndex = ValueSet.ToExamineXml();
            }
        }

        public IndexItem(string type, string id)
        {
            IndexType = type;
            Id = id;
        }

        public IndexItem(ValueSet valueSet)
        {
            ValueSet = valueSet;
            IndexType = ValueSet.Type;
            Id = ValueSet.Id.ToString();            
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [Obsolete("Use ValueSets instead")]
        public XElement DataToIndex
        {
            get { InitializeLegacyFields(); return _dataToIndex; }
            private set { _dataToIndex = value; }
        }

        /// <summary>
        /// Gets or sets the type of the index.
        /// </summary>
        /// <value>
        /// The type of the index.
        /// </value>        
        public string IndexType { get; private set; }        

        /// <summary>
        /// Gets the id.
        /// </summary>        
        public string Id { get; private set; }        
    }
}