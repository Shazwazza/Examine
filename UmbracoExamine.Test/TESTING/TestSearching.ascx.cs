using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UmbracoExamine.Providers;
using UmbracoExamine.Core;
using System.Drawing;

namespace UmbracoExamine.Test.TESTING
{
    public partial class TestSearching : TestControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void TestSearch_Click(object sender, EventArgs e)
        {
            var results = ExamineManager.Instance.Search(SearchTextBox.Text, 100, true);


            foreach (var r in results)
            {
                AddTrace("Result", string.Format("Score: {0}, NodeId: {1}", r.Score, r.Id), Color.Black);
            }

        }
    }
}