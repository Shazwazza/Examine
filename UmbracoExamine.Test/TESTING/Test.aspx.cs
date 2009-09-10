using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UmbracoExamine.Core;
using UmbracoExamine.Providers;
using System.Drawing;

namespace UmbracoExamine.Test.Testing
{
    public partial class Test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
            

            
        }

        

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            TraceTable = new Table();
            TableRow head = new TableRow();
            head.CssClass = "head";
            TableCell cat = new TableCell();
            cat.Text = "Category";
            cat.CssClass = "cat";
            TableCell msg = new TableCell();
            msg.Text = "Message";
            head.Cells.Add(cat);
            head.Cells.Add(msg);
            TraceTable.Rows.Add(head);            
        }

        protected Table TraceTable;

        

        public void AddTrace(string category, string message, Color color)
        {
            EnsureChildControls();
            TableRow row = new TableRow();
            row.CssClass = TraceTable.Rows.Count % 2 == 0 ? "alt" : "";
            row.ForeColor = color;
            TableCell cat = new TableCell();
            TableCell msg = new TableCell();
            row.Cells.Add(cat);
            cat.Text = category;
            cat.CssClass = "cat";
            row.Cells.Add(msg);
            msg.Text = message;
            TraceTable.Rows.Add(row);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            TraceOutput.Controls.Add(TraceTable);
        }
    }
}
