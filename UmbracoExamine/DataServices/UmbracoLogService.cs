using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.BusinessLogic;

namespace UmbracoExamine.DataServices
{
    public class UmbracoLogService : UmbracoExamine.DataServices.ILogService
    {

        public void AddInfoLog(int nodeId, string msg)
        {
            Log.Add(LogTypes.Custom, nodeId, "[UmbracoExamine] " + msg);
        }

        public void AddErrorLog(int nodeId, string msg)
        {
            Log.Add(LogTypes.Error, nodeId, "[UmbracoExamine] " + msg);
        }

        public void AddVerboseLog(int nodeId, string msg)
        {
            if (LogLevel == LoggingLevel.Verbose)
                Log.Add(LogTypes.Custom, nodeId, "[UmbracoExamine] " + msg);
        }

        public LoggingLevel LogLevel { get; set; }

    }
}
