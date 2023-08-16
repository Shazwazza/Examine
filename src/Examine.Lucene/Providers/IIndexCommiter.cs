using System;

namespace Examine.Lucene.Providers
{
    /// <summary>
    /// This queues up a commit for the index so that a commit doesn't happen on every individual write since that is quite expensive
    /// </summary>
    public interface IIndexCommiter : IDisposable
    {
        /// <summary>
        /// Commits the index to directory
        /// </summary>
        void CommitNow();

        /// <summary>
        /// Schedules the index to be commited to the directory
        /// </summary>
        void ScheduleCommit();
    }
}
