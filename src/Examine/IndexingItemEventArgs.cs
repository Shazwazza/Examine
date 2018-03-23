using System;

namespace Examine
{
    public class IndexingItemEventArgs : EventArgs
    {
        public IIndexer Indexer { get; }
        public IndexItem IndexItem { get; }

        public IndexingItemEventArgs(IIndexer indexer, IndexItem indexItem)
        {
            Indexer = indexer;
            IndexItem = indexItem;
        }
    }
}