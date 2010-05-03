using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.DataServices;

namespace Examine.Test.DataServices
{
    public class TestLogService : ILogService
    {
        #region ILogService Members

        public void AddErrorLog(int nodeId, string msg)
        {
            //do nothing
        }

        public void AddInfoLog(int nodeId, string msg)
        {
            //do nothing
        }

        #endregion
    }
}
