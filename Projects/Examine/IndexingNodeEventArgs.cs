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
        public ValueSet Values { get; private set; }
        //public IndexingNodeEventArgs(int nodeId, Dictionary<string, string> fields, string indexType)
        
        public IndexingNodeEventArgs(ValueSet values)
        {
            Values = values;


            //Legacy stuff
            NodeId = (int) values.Id;
            IndexType = values.Type;

            Fields = values.ToLegacyFields();
        }

        [Obsolete("Use ValueSet instead")]
        public IndexingNodeEventArgs(int nodeId, Dictionary<string, string> fields, string indexType)
            : this( ValueSet.FromLegacyFields(nodeId, indexType, fields))            
        {                        
        }

        [Obsolete("Use ValueSet instead")]
        public int NodeId { get; private set; }
        [Obsolete("Use ValueSet instead")]
        public Dictionary<string, string> Fields { get; private set; }
        [Obsolete("Use ValueSet instead")]
        public string IndexType { get; private set; }
    }
}