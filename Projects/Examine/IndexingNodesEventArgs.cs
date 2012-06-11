using System.ComponentModel;


namespace Examine
{
    public class IndexingNodesEventArgs : CancelEventArgs
    {

        public IndexingNodesEventArgs(IIndexCriteria indexData, string xPath, string type)
        {
            this.IndexData = indexData;
            this.XPath = xPath;
            this.Type = type;
        }

        public IIndexCriteria IndexData { get; private set; }
        public string XPath { get; private set; }
        public string Type {get; private set;}

    }
}