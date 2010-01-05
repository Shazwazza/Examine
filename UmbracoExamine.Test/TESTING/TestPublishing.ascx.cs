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
            AddTrace("Consecutive-Publishing", "Start publishing many nodes consecutively", Color.Green);
            PublishAllNodes();
        }

        protected void TestSinglePublish_Click(object sender, EventArgs e)
        {
            AddTrace("Single-Publishing", "Start publishing one nodes", Color.Green);
            var doc = Document.GetRootDocuments().ToList().First();
            doc.Publish(m_Admin);
            umbraco.library.UpdateDocumentCache(doc.Id);
        }

        User m_Admin = new User(0);

        /// <summary>
        /// This will individual publish all nodes in the tree
        /// </summary>
        private void PublishAllNodes()
        {
            var docs = Document.GetRootDocuments().ToList();
            docs.ForEach(x =>
            {
                PublishNode(x);
            });
        }

        private void PublishNode(Document x)
        {
            AddTrace("Multi-Publishing", "Publishing node: " + x.Id.ToString(), Color.Magenta);
            x.Publish(m_Admin);
            umbraco.library.UpdateDocumentCache(x.Id);

            //check for children and recurse
            if (x.HasChildren)
            {
                x.Children.ToList().ForEach(d =>
                {
                    PublishNode(d);
                });
            }
        }
    }
}