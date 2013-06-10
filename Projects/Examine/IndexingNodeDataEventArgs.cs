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

        public IndexingNodeDataEventArgs(XElement node, int nodeId, Dictionary<string, string> fields, string indexType)
            : base(nodeId, fields, indexType)
        {
            this.Node = node;
        }

        public IndexingNodeDataEventArgs(ValueSet valueSet)
            : base(valueSet)
        {
            this.Node = valueSet.ToExamineXml();
        }

        [Obsolete("Use ValueSet instead")]
        public XElement Node { get; private set; }
    }
}
