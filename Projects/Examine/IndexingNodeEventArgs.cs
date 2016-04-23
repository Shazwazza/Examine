using System;
using System.ComponentModel;
using System.Collections.Generic;
using Examine.LuceneEngine.Indexing;
using System.Linq;

namespace Examine
{
    [Obsolete("Use the TransformIndexValues event with TransformingIndexDataEventArgs instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IndexingNodeEventArgs : CancelEventArgs, INodeEventArgs
    {
        private Dictionary<string, string> _fields;
        
        /// <summary>
        /// The value's for the node indexing
        /// </summary>
        public ValueSet Values { get; private set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="values"></param>
        public IndexingNodeEventArgs(ValueSet values)
        {
            Values = values;

            //Legacy stuff
            NodeId = (int) values.Id;
            IndexType = values.IndexCategory;            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="fields"></param>
        /// <param name="indexCategory"></param>
        [Obsolete("Do not use this constructor, it doesn't contain enough information to creaet a ValueSet")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IndexingNodeEventArgs(int nodeId, Dictionary<string, string> fields, string indexCategory)
            //This ctor doesn't contain the item type!
            : this( ValueSet.FromLegacyFields(nodeId, indexCategory, "", fields))
        {
            _fields = fields;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="fields"></param>
        /// <param name="indexCategory"></param>
        /// <param name="itemType"></param>
        [Obsolete("Use ValueSet instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IndexingNodeEventArgs(int nodeId, Dictionary<string, string> fields, string indexCategory, string itemType)
            : this(ValueSet.FromLegacyFields(nodeId, indexCategory, itemType, fields))
        {
            _fields = fields;
        }

        void InitializeLegacyData()
        {
            if (_fields == null)
            {
                _fields = Values.ToLegacyFields();
            }
        }

        /// <summary>
        /// The node id being indexed
        /// </summary>
        [Obsolete("Use ValueSet instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int NodeId { get; private set; }

        /// <summary>
        /// The values being indexed
        /// </summary>
        [Obsolete("Use ValueSet instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Dictionary<string, string> Fields
        {
            get { InitializeLegacyData(); return _fields; }            
        }

        /// <summary>
        /// The index category
        /// </summary>
        [Obsolete("Use ValueSet instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string IndexType { get; private set; }
    }
}