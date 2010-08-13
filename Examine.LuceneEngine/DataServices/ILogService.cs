using System;
namespace LuceneExamine.DataServices
{
    public interface ILogService
    {
        void AddErrorLog(int nodeId, string msg);
        void AddInfoLog(int nodeId, string msg);
    }
}
