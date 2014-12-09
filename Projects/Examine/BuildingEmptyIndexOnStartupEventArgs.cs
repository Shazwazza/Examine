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
        /// Contructor
        /// </summary>
        /// <param name="indexer"></param>
        public BuildingEmptyIndexOnStartupEventArgs(IIndexer indexer)
        {
            Indexer = indexer;
        }
    }
}