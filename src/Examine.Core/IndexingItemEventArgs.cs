using System.ComponentModel;

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
        /// Initializes a new instance of the <see cref="IndexingItemEventArgs" /> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="valueSet">The value set.</param>
        public IndexingItemEventArgs(IIndex index, ValueSet valueSet)
        {
            Index = index;
            ValueSet = valueSet;
        }
    }
}
