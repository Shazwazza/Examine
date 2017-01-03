using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Examine
{
    public class IndexingFieldDataEventArgs : EventArgs, INodeEventArgs
    {

        public IndexingFieldDataEventArgs(XElement node, string fieldName, string fieldValue, bool isStandardField, int nodeId)
        {
            this.Node = node;
            this.FieldName = fieldName;
            this.FieldValue = fieldValue;
            this.IsStandardField = isStandardField;
            this.NodeId = nodeId;
        }

        public XElement Node { get; private set; }
        public string FieldName { get; private set; }
        public string FieldValue { get; private set; }
        public bool IsStandardField { get; private set; }

        #region INodeEventArgs Members

        public int NodeId { get; private set; }

        #endregion
    }
}
