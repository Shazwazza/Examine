using System;
using System.Web;
using System.Web.UI;
using System.Drawing;
using UmbracoExamine.Core;
using UmbracoExamine.Providers;

namespace UmbracoExamine.Test.TESTING
{
    public class TestControl : UserControl
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (EventsRegistered)
                return;

            foreach (BaseIndexProvider indexer in ExamineManager.Instance.IndexProviderCollection)
            {
                ExamineManager.Instance.IndexProviderCollection[indexer.Name].NodesIndexing += new EventHandler<IndexingNodesEventArgs>(TestControl_NodesIndexing);
                ExamineManager.Instance.IndexProviderCollection[indexer.Name].NodeIndexed += new EventHandler<IndexedNodeEventArgs>(TestControl_NodeIndexed);
                ExamineManager.Instance.IndexProviderCollection[indexer.Name].IndexingError +=new EventHandler<IndexingErrorEventArgs>(TestControl_IndexingError);
                ExamineManager.Instance.IndexProviderCollection[indexer.Name].GatheringNodeData +=new EventHandler<IndexingNodeDataEventArgs>(TestControl_GatheringNodeData);
                ExamineManager.Instance.IndexProviderCollection[indexer.Name].IndexDeleted +=new EventHandler<DeleteIndexEventArgs>(TestControl_IndexDeleted);
            }

            EventsRegistered = true;
        }

        void TestControl_GatheringNodeData(object sender, IndexingNodeDataEventArgs e)
        {
            foreach (var entry in e.Values)
            {
                AddTrace("Gathering Index Data", string.Format("DATA: {0} : {1}", entry.Key, entry.Value), Color.Brown);
            }
        }

        void TestControl_IndexingError(object sender, IndexingErrorEventArgs e)
        {
            AddTrace("Indexing Error", string.Format("{0} : {1},{2}", e.NodeId, e.Message, e.InnerException != null ? e.InnerException.Message : ""), Color.Red);
            Page.ClientScript.RegisterStartupScript(typeof(TestControl), "ErrorAlert", "alert('There were errors');", true);
        }

        void TestControl_NodeIndexed(object sender, IndexedNodeEventArgs e)
        {
            AddTrace("Node Indexed", string.Format("Node {0} Index with Provider: {1}", e.NodeId, ((BaseIndexProvider)sender).Name), Color.Blue);
        }

        void TestControl_NodesIndexing(object sender, IndexingNodesEventArgs e)
        {
            AddTrace("Nodes Indexing", string.Format("Indexing " + e.Type.ToString().ToUpper() + " nodes with Provider: {0} and XPath statement: {1}", ((BaseIndexProvider)sender).Name, e.XPath), Color.Black);
        }

        void TestControl_IndexDeleted(object sender, DeleteIndexEventArgs e)
        {
            AddTrace("Index Deleted", string.Format("Term: {0} with value {1}", e.DeletedTerm.Key, e.DeletedTerm.Value), Color.Purple);
        }

        protected bool EventsRegistered
        {
            get
            {
                if (HttpContext.Current.Items["eventsRegistered"] != null)
                    return bool.Parse(HttpContext.Current.Items["eventsRegistered"].ToString());
                return false;
            }
            set
            {
                HttpContext.Current.Items["eventsRegistered"] = value;
            }
        }

        protected void AddTrace(string category, string message, Color color)
        {
            ((UmbracoExamine.Test.Testing.Test)Page).AddTrace(category, message, color);
        }

        protected void AddTrace(TraceMessage msg)
        {
            ((UmbracoExamine.Test.Testing.Test)Page).AddTrace(msg.Category, msg.Message, msg.Color);
        }

    }
}