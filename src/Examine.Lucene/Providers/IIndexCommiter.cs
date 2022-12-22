using System;

namespace Examine.Lucene.Providers
{
    public interface IIndexCommiter : IDisposable
    {
        void CommitNow();
        void ScheduleCommit();
    }
}
