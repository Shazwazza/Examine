using System;
namespace UmbracoExamine.DataServices
{
    public interface ILogService
    {
        void AddErrorLog(int nodeId, string msg);
        void AddInfoLog(int nodeId, string msg);
    }
}
