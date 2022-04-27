using System;
using System.ComponentModel;

namespace Examine
{
    public class IndexingItemEventArgs : CancelEventArgs
    {
        private ValueSet _valueSet;

        public IIndex Index { get; }

        public ValueSet ValueSet
        {
            get => _valueSet;
            set => _valueSet = value ?? throw new ArgumentNullException("value");
        }

        public IndexingItemEventArgs(IIndex index, ValueSet valueSet)
        {
            Index = index;
            ValueSet = valueSet;
        }
    }
}
