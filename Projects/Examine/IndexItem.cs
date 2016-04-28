using System;
using System.ComponentModel;
using System.Globalization;
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

        /// <summary>
        /// Exposes the underlying ValueSet
        /// </summary>
        public ValueSet ValueSet { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexItem"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="indexCategory">The type.</param>
        /// <param name="id">The id.</param>
        [Obsolete("Use ValueSets instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IndexItem(XElement data, string indexCategory, string id)
           : this(data.ToValueSet(indexCategory, data.ExamineNodeTypeAlias(), long.Parse(id)))
        {
            DataToIndex = data;
            IndexType = indexCategory;
            Id = id;            
        }

        void InitializeLegacyFields()
        {
            if (_dataToIndex == null && ValueSet != null)
            {
                _dataToIndex = ValueSet.ToExamineXml();
            }
        }

        internal void ResetLegacyFields()
        {
            _dataToIndex = null;
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="valueSet"></param>        
        public IndexItem(ValueSet valueSet)
        {
            ValueSet = valueSet;

            IndexType = ValueSet.IndexCategory;
            Id = ValueSet.Id.ToString(CultureInfo.InvariantCulture);            
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [Obsolete("Use ValueSets instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public XElement DataToIndex
        {
            get { InitializeLegacyFields(); return _dataToIndex; }
            private set { _dataToIndex = value; }
        }

        [Obsolete("Use ValueSet.IndexCategory instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string IndexType { get; private set; }

        [Obsolete("Use ValueSet.Id")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string Id { get; private set; }        
    }
}