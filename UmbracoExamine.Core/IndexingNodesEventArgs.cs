using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;


namespace UmbracoExamine.Core
{
    public class IndexingNodesEventArgs : EventArgs
    {

        public IndexingNodesEventArgs(IIndexCriteria indexData, string xPath)
        {
            this.IndexData = indexData;
            this.XPath = xPath;
        }

        public IIndexCriteria IndexData { get; private set; }
        public string XPath { get; private set; }

    }
}
