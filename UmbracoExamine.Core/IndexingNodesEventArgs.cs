using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;


namespace UmbracoExamine.Core
{
    public class IndexingNodesEventArgs : EventArgs
    {

        public IndexingNodesEventArgs(IIndexCriteria indexData, string xPath, IndexType type)
        {
            this.IndexData = indexData;
            this.XPath = xPath;
            this.Type = type;
        }

        public IIndexCriteria IndexData { get; private set; }
        public string XPath { get; private set; }
        public IndexType Type {get; private set;}

    }
}
