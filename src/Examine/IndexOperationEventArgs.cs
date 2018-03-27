using System;

namespace Examine
{
    public class IndexOperationEventArgs : EventArgs
    {
        public IIndexer Indexer { get; }
        public int ItemsIndexed { get; }

        public IndexOperationEventArgs(IIndexer indexer, int itemsIndexed)
        {
            Indexer = indexer;
            ItemsIndexed = itemsIndexed;
        }
    }
}