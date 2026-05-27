using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Examine
{
    public class IndexingItemEventArgs : CancelEventArgs
    {
        public IIndex Index { get; }

        public ValueSet ValueSet { get; private set; }

        public IndexingItemEventArgs(IIndex index, ValueSet valueSet)
        {
            Index = index;
            ValueSet = valueSet;
        }

        public void SetValues(IDictionary<string, IEnumerable<object>> values)
            => ValueSet = new ValueSet(ValueSet.Id, ValueSet.Category, ValueSet.ItemType, values);
    }
}
