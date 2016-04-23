using System;
using Examine.LuceneEngine.Indexing;

namespace Examine
{
    public class IndexItemEventArgs : EventArgs
    {
        public IndexItem IndexItem { get; private set; }

        public IndexItemEventArgs(IndexItem item)
        {
            IndexItem = item;
        }
    }
}