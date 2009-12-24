using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Drawing;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic;

namespace UmbracoExamine.Test.TESTING
{
    public partial class TestPublishing : TestControl
    {
        
        protected void TestMultiplePublish_Click(object sender, EventArgs e)
        {
            AddTrace("Multi-Publishing", "Start publishing many nodes", Color.Green);
            PublishAllNodes();
        }

        private void PublishAllNodes()
        {
            var docs = Document.GetRootDocuments().ToList();
            var admin = new User(0);
            for (int i = 0; i < 50; i++)
            {
                docs.ForEach(x =>
                {
                    AddTrace("Multi-Publishing", "Publishing node: " + ID.ToString(), Color.Magenta);
                    x.Publish(admin);
                    umbraco.library.UpdateDocumentCache(x.Id);
                });
            }            
        }
    }
}