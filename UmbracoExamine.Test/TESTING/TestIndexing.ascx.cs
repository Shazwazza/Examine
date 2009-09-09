using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UmbracoExamine.Core;

namespace UmbracoExamine.Test.TESTING
{
    public partial class TestIndexing : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ExamineManager.Instance.DefaultIndexProvider.NodesIndexing += new UmbracoExamine.Providers.BaseIndexProvider.IndexingNodesEventHandler(DefaultIndexProvider_NodesIndexing);
            ExamineManager.Instance.DefaultIndexProvider.NodeIndexed += new UmbracoExamine.Providers.BaseIndexProvider.IndexingNodeEventHandler(DefaultIndexProvider_NodeIndexed);            
        }

        void DefaultIndexProvider_NodeIndexed(object sender, IndexingNodeEventArgs e)
        {
            Trace.Warn("TestIndexing", "DefaultIndexProvider_NodeIndexed");
            Trace.Warn("TestIndexing", e.NodeId.ToString());
        }

        string DefaultIndexProvider_NodesIndexing(object sender, IndexingNodesEventArgs e)
        {
            Trace.Warn("TestIndexing", "DefaultIndexProvider_NodesIndexing");
            Trace.Warn("TestIndexing", e.XPath);
            return e.XPath;
        }

        protected void TestIndexButton_Click(object sender, EventArgs e)
        {
            ExamineManager.Instance.IndexAll();
        }


    }
}