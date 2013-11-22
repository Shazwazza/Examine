using System;
using System.ComponentModel;
using System.Collections.Generic;
using Examine.LuceneEngine.Indexing;
using LuceneManager.Infrastructure;
using System.Linq;

namespace Examine
{
    /// <summary>
    /// Event arguments for when an item is indexing
    /// </summary>
    public class IndexingNodeEventArgs : CancelEventArgs, INodeEventArgs
    {
        private Dictionary<string, string> _fields;
        
        /// <summary>
        /// The value's for the node indexing
        /// </summary>
        public ValueSet Values { get; private set; }
        //public IndexingNodeEventArgs(int nodeId, Dictionary<string, string> fields, string indexType)
        
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
        public int NodeId { get; private set; }

        /// <summary>
        /// The values being indexed
        /// </summary>
        [Obsolete("Use ValueSet instead")]
        public Dictionary<string, string> Fields
        {
            get { InitializeLegacyData(); return _fields; }            
        }

        /// <summary>
        /// The index category
        /// </summary>
        [Obsolete("Use ValueSet instead")]
        public string IndexType { get; private set; }
    }
}