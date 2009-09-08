using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace UmbracoExamine.Core
{
    public class IndexingFieldDataEventArgs : IndexingNodeEventArgs
    {

        public IndexingFieldDataEventArgs(XElement node, string fieldName, string fieldValue, bool isUmbracoField, int nodeId)
            : base(nodeId)
        {
            this.Node = node;
            this.FieldName = fieldName;
            this.FieldValue = fieldValue;
            this.IsUmbracoField = isUmbracoField;
        }

        public XElement Node { get; private set; }
        public string FieldName { get; private set; }
        public string FieldValue { get; private set; }
        public bool IsUmbracoField { get; private set; }


    }
}
