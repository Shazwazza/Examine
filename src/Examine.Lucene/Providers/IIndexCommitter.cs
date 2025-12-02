using System;

namespace Examine.Lucene.Providers
{
    /// <summary>  
    /// This queues up a commit for the index so that a commit doesn't happen on every individual write since that is quite expensive  
    /// </summary>  
    public interface IIndexCommitter : IDisposable
    {
        /// <summary>  
        /// Commits the index to directory  
        /// </summary>  
        public void CommitNow();

        /// <summary>  
        /// Schedules the index to be committed to the directory  
        /// </summary>  
        public void ScheduleCommit();

        /// <summary>  
        /// Occurs when an error happens during the commit process.  
        /// </summary>  
        public event EventHandler<IndexingErrorEventArgs> CommitError;

        /// <summary>  
        /// Occurs when the index has been successfully committed.  
        /// </summary>  
        public event EventHandler? Committed;
    }
}
