using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine;

namespace Examine
{
    /// <summary>
    /// Event args representing node data indexing
    /// </summary>    
    [Obsolete("Use the TransformIndexValues event with TransformingIndexDataEventArgs instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IndexingNodeDataEventArgs : IndexingNodeEventArgs
    {
        private XElement _node;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeId"></param>
        /// <param name="fields"></param>
        /// <param name="indexType"></param>
        [Obsolete("Use ValueSet instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IndexingNodeDataEventArgs(XElement node, int nodeId, Dictionary<string, string> fields, string indexType)
            : base(nodeId, fields, indexType, node.ExamineNodeTypeAlias())
        {
            this.Node = node;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="valueSet"></param>
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
        [EditorBrowsable(EditorBrowsableState.Never)]
        public XElement Node

        {
            get { InitializeLegacyData(); return _node; }
            private set { _node = value; }
        }
    }
}
