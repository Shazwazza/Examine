using System;
using System.ComponentModel;
using System.Collections.Generic;
using Examine.LuceneEngine.Indexing;
using LuceneManager.Infrastructure;
using System.Linq;

namespace Examine
{
    public class IndexingNodeEventArgs : CancelEventArgs, INodeEventArgs
    {
        private Dictionary<string, string> _fields;
        public ValueSet Values { get; private set; }
        //public IndexingNodeEventArgs(int nodeId, Dictionary<string, string> fields, string indexType)
        
        public IndexingNodeEventArgs(ValueSet values)
        {
            Values = values;


            //Legacy stuff
            NodeId = (int) values.Id;
            IndexType = values.Type;            
        }

        [Obsolete("Use ValueSet instead")]
        public IndexingNodeEventArgs(int nodeId, Dictionary<string, string> fields, string indexType)
            : this( ValueSet.FromLegacyFields(nodeId, indexType, fields))
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

        [Obsolete("Use ValueSet instead")]
        public int NodeId { get; private set; }
        [Obsolete("Use ValueSet instead")]
        public Dictionary<string, string> Fields
        {
            get { InitializeLegacyData(); return _fields; }            
        }

        [Obsolete("Use ValueSet instead")]
        public string IndexType { get; private set; }
    }
}