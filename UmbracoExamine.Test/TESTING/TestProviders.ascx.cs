using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UmbracoExamine.Core;
using UmbracoExamine.Providers;

namespace UmbracoExamine.Test.TESTING
{
    public partial class TestProviders : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void TestProviderButton_Click(object sender, EventArgs e)
        {
            foreach (BaseIndexProvider indexer in ExamineManager.Instance.IndexProviderCollection)
            {
                Trace.Warn("TestProviders", indexer.Name + " : " + indexer.Description);
            }            
        }
    }
}