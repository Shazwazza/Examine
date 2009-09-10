using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Drawing;

namespace UmbracoExamine.Test.TESTING
{
    public class TestControl : UserControl
    {

        

        protected void AddTrace(string category, string message, Color color)
        {
            ((UmbracoExamine.Test.Testing.Test)Page).AddTrace(category, message, color);
        }

    }
}
