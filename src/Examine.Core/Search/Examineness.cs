
using System;

namespace Examine.Search
{
    /// <summary>
    /// Different ways to match terms
    /// </summary>
    public enum Examineness
    {
        /// <summary>
        /// A normal field query
        /// </summary>
        Default = 100,

        /// <summary>
        /// Matches terms using 'fuzzy' logic
        /// </summary>
        Fuzzy = 0,

        /// <summary>
        /// Wildcard matching a single character
        /// </summary>
        SimpleWildcard = 1,

        /// <summary>
        /// Wildcard matching multiple characters
        /// </summary>
        ComplexWildcard = 2,

        /// <summary>
        /// A normal field query
        /// </summary>
        [Obsolete("Use default instead")]
        Explicit = 3,

        /// <summary>
        /// Becomes exact match
        /// </summary>
        [Obsolete("Use phrase instead")]
        Escaped = 4,

        /// <summary>
        /// Makes the term rank differently than normal
        /// </summary>
        [Obsolete("No longer used, use WithBoost instead.")]
        Boosted = 5,

        /// <summary>
        /// Searches for terms within a proximity of each other
        /// </summary>
        Proximity = 6,

        /// <summary>
        /// Makes the term a phrase query
        /// </summary>
        Phrase = 7
    }
}
