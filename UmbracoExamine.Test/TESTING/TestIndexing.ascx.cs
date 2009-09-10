using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UmbracoExamine.Core;
using UmbracoExamine.Providers;
using System.Drawing;

namespace UmbracoExamine.Test.TESTING
{
    public partial class TestIndexing : TestControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            foreach (BaseIndexProvider indexer in ExamineManager.Instance.IndexProviderCollection)
            {

                ExamineManager.Instance.IndexProviderCollection[indexer.Name].NodesIndexing += new BaseIndexProvider.IndexingNodesEventHandler(TestIndexing_NodesIndexing);
                ExamineManager.Instance.IndexProviderCollection[indexer.Name].NodeIndexed += new BaseIndexProvider.IndexingNodeEventHandler(TestIndexing_NodeIndexed);
                ExamineManager.Instance.IndexProviderCollection[indexer.Name].IndexingError += new BaseIndexProvider.IndexingErrorEventHandler(TestIndexing_IndexingError);
                ExamineManager.Instance.IndexProviderCollection[indexer.Name].GatheringNodeData += new BaseIndexProvider.IndexingNodeDataEventHandler(TestIndexing_GatheringNodeData);
            }
            
            
        }

        Dictionary<string, string> TestIndexing_GatheringNodeData(object sender, IndexingNodeDataEventArgs e)
        {
            foreach (var entry in e.Values)
            {
                AddTrace("Gathering Index Data", string.Format("DATA: {0} : {1}", entry.Key, entry.Value), Color.Brown);
            }
            return e.Values;
        }

        void TestIndexing_IndexingError(object sender, IndexingErrorEventArgs e)
        {
            AddTrace("Indexing Error", string.Format("{0} : {1},{2}", e.NodeId, e.Message, e.InnerException != null ? e.InnerException.Message : ""), Color.Red);
        }

        void TestIndexing_NodeIndexed(object sender, IndexingNodeEventArgs e)
        {

            AddTrace("Node Indexed", string.Format("Node {0} Index with Provider: {1}", e.NodeId, ((BaseIndexProvider)sender).Name), Color.Blue);
        }

        string TestIndexing_NodesIndexing(object sender, IndexingNodesEventArgs e)
        {
            AddTrace("Nodes Indexing", string.Format("Indexing nodes with Provider: {0} and XPath statement: {1}", ((BaseIndexProvider)sender).Name, e.XPath), Color.Black);
            return e.XPath;
        }

        protected void TestIndexButton_Click(object sender, EventArgs e)
        {
            AddTrace("Indexing Content", "Start all content indexing", Color.Green);
            ExamineManager.Instance.IndexAll(IndexType.Content);
            AddTrace("Indexing Media", "Start all media indexing", Color.Green);
            ExamineManager.Instance.IndexAll(IndexType.Media);
        }

        protected void TestRebuildButton_Click(object sender, EventArgs e)
        {
            AddTrace("Rebuilding Index", "Start rebuilding the indexes", Color.Green);
            ExamineManager.Instance.RebuildIndex();
        }
        

    }
}