using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Examine
{
    /// <summary>
    /// Represents indexing item event arguments
    /// </summary>
    public class IndexingItemEventArgs : CancelEventArgs
    {
        /// <summary>
        /// The index of the event
        /// </summary>
        public IIndex Index { get; }

        /// <summary>
        /// The value set of the event
        /// </summary>
        public ValueSet ValueSet { get; private set; }

        /// <inheritdoc/>
        public IndexingItemEventArgs(IIndex index, ValueSet valueSet)
        {
            Index = index;
            ValueSet = valueSet;
        }

        /// <summary>
        /// Sets the value of the <see cref="ValueSet"/>
        /// </summary>
        /// <param name="values"></param>
        public void SetValues(IDictionary<string, IEnumerable<object>> values)
            => ValueSet = new ValueSet(ValueSet.Id, ValueSet.Category, ValueSet.ItemType, values);
    }
}
