using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Examine
{
    public class IndexingItemEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Gets the index.
        /// </summary>
        public IIndex Index { get; }

        /// <summary>
        /// Gets the value set.
        /// </summary>
        public ValueSet ValueSet { get; }

        /// <summary>
        /// Gets the transformed values to index.
        /// </summary>
        public IDictionary<string, IList<object>> TransformedValues { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexingItemEventArgs" /> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="valueSet">The value set.</param>
        public IndexingItemEventArgs(IIndex index, ValueSet valueSet)
        {
            Index = index;
            ValueSet = valueSet;
            TransformedValues = valueSet.Values.ToDictionary<KeyValuePair<string, IReadOnlyList<object>>, string, IList<object>>(x => x.Key, x => x.Value.ToList());
        }
    }
}
