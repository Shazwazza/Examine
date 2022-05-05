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

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="values">The values.</param>
        [Obsolete("Set the values on the TransformedValues property instead to prevent unnecessary allocations.")]
        public void SetValues(IDictionary<string, IEnumerable<object>> values)
        {
            TransformedValues.Clear();

            if (values != null)
            {
                foreach (KeyValuePair<string, IEnumerable<object>> value in values)
                {
                    TransformedValues.Add(value.Key, value.Value.ToList());
                }
            }
        }
    }
}
