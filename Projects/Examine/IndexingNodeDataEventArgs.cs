using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine;

namespace Examine
{
    public class IndexingNodeDataEventArgs : IndexingNodeEventArgs
    {
        private XElement _node;

        public IndexingNodeDataEventArgs(XElement node, int nodeId, Dictionary<string, string> fields, string indexType)
            : base(nodeId, fields, indexType)
        {
            this.Node = node;
        }

        public IndexingNodeDataEventArgs(ValueSet valueSet)
            : base(valueSet)
        {
            
        }

        private void InitializeLegacyData()
        {
            if (_node == null)
            {
                _node = Values.ToExamineXml();
            }
        }

        [Obsolete("Use ValueSet instead")]
        public XElement Node

        {
            get { InitializeLegacyData(); return _node; }
            private set { _node = value; }
        }
    }
}
