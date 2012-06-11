
namespace Examine.SearchCriteria
{

    /// <summary>
    /// Different ways to match terms
    /// </summary>
    public enum Examineness
    {
        /// <summary>
        /// Matches terms using 'fuzzy' logic
        /// </summary>
        Fuzzy, 

        /// <summary>
        /// Wildcard matching a single character
        /// </summary>
        SimpleWildcard, 

        /// <summary>
        /// Wildcard matching multiple characters
        /// </summary>
        ComplexWildcard, 
        
        /// <summary>
        /// A normal phrase query
        /// </summary>
        Explicit, 
        
        /// <summary>
        /// Becomes exact match
        /// </summary>
        Escaped, 

        /// <summary>
        /// Makes the term rank differently than normal
        /// </summary>
        Boosted, 

        /// <summary>
        /// Searches for terms within a proximity of each other
        /// </summary>
        Proximity


    }
}
