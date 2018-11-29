using System;

namespace Examine
{
    public class IndexOperationEventArgs : EventArgs
    {
        public IIndex Index { get; }
        public int ItemsIndexed { get; }

        public IndexOperationEventArgs(IIndex index, int itemsIndexed)
        {
            Index = index;
            ItemsIndexed = itemsIndexed;
        }
    }
}