using System;
using System.ComponentModel;

namespace Examine
{
    public class IndexingItemEventArgs : CancelEventArgs
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