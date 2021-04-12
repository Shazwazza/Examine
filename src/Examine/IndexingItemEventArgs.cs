using System.ComponentModel;

namespace Examine
{
    public class IndexingItemEventArgs : CancelEventArgs
    {
        public IIndex Index { get; }
        public ValueSet ValueSet { get; }

        public IndexingItemEventArgs(IIndex index, ValueSet valueSet)
        {
            Index = index;
            ValueSet = valueSet;
        }
    }
}