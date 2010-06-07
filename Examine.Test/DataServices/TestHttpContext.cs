using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.DataServices;
using System.Web;
using System.IO;
using System.Reflection;

namespace Examine.Test.DataServices
{
    class TestHttpContext : HttpContextBase
    {
        private HttpServerUtilityBase _server;
        public TestHttpContext()
        {
            _server = new TestHttpServerUtility();
        }

        public override HttpServerUtilityBase Server
        {
            get
            {
                return _server;
            }
        }
    }

    class TestHttpServerUtility : HttpServerUtilityBase
    {
        public override string MapPath(string path)
        {
            return new FileInfo(Assembly.GetExecutingAssembly().Location).Directory + "\\" + path.Replace("/", "\\");
        }
    }
}
