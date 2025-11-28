
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
        Default = 0,

        /// <summary>
        /// Matches terms using 'fuzzy' logic
        /// </summary>
        Fuzzy = 1,

        /// <summary>
        /// Wildcard matching a single character
        /// </summary>
        SimpleWildcard = 2,

        /// <summary>
        /// Wildcard matching multiple characters
        /// </summary>
        ComplexWildcard = 3,

        /// <summary>
        /// A normal field query
        /// </summary>
        [Obsolete("Use default instead")]
        Explicit = 4,

        /// <summary>
        /// Becomes exact match
        /// </summary>
        [Obsolete("Use phrase instead")]
        Escaped = 5,

        /// <summary>
        /// Makes the term rank differently than normal
        /// </summary>
        /// TODO: Should be obsolete
        Boosted = 6,

        /// <summary>
        /// Searches for terms within a proximity of each other
        /// </summary>
        Proximity = 7,

        /// <summary>
        /// Makes the term a phrase query
        /// </summary>
        Phrase = 8
    }
}
