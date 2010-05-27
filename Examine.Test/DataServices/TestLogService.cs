using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.DataServices;
using System.Diagnostics;

namespace Examine.Test.DataServices
{
    public class TestLogService : ILogService
    {
        #region ILogService Members

        public void AddErrorLog(int nodeId, string msg)
        {
            Trace.WriteLine("ERROR: (" + nodeId.ToString() + ") " + msg);
        }

        public void AddInfoLog(int nodeId, string msg)
        {
            Trace.WriteLine("INFO: (" + nodeId.ToString() + ") " + msg);
        }

        #endregion
    }
}
