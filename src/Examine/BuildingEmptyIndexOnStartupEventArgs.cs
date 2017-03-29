using System;
using System.ComponentModel;

namespace Examine
{
    /// <summary>
    /// Event args used during app startup for the BuildingEmptyIndexOnStartup event
    /// </summary>
    public class BuildingEmptyIndexOnStartupEventArgs : CancelEventArgs
    {
        /// <summary>
        /// The indexer
        /// </summary>
        public IIndexer Indexer { get; private set; }

        /// <summary>
        /// If the index is readable
        /// </summary>
        public bool IsHealthy { get; private set; }

        /// <summary>
        /// The exception given if its not readable
        /// </summary>
        public Exception UnhealthyException { get; private set; }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="indexer"></param>
        public BuildingEmptyIndexOnStartupEventArgs(IIndexer indexer)
        {
            Indexer = indexer;
            IsHealthy = true;
        }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="indexer"></param>
        /// <param name="isHealthy"></param>
        /// <param name="unhealthyException"></param>
        public BuildingEmptyIndexOnStartupEventArgs(IIndexer indexer, bool isHealthy, Exception unhealthyException)
        {
            Indexer = indexer;
            IsHealthy = isHealthy;
            UnhealthyException = unhealthyException;
        }
    }
}