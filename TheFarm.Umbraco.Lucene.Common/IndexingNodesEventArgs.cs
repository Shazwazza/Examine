using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.Configuration;
using System.Xml.Linq;


namespace UmbracoExamine
{
    public class IndexingNodesEventArgs : EventArgs
    {

        public IndexingNodesEventArgs(IndexerData indexData, string xPath)
        {
            this.IndexData = indexData;
            this.XPath = xPath;
        }

        public IndexerData IndexData { get; private set; }
        public string XPath { get; private set; }

    }
}
