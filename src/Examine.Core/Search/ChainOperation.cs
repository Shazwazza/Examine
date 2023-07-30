namespace Examine.Search
{
    /// <summary>
    /// Represents types of chain operations
    /// </summary>
    public enum ChainOperation
    {
        /// <summary>
        /// Or
        /// </summary>
        OR = 0,
        /// <summary>
        /// And
        /// </summary>
        AND = 1,
        /// <summary>
        /// And Not
        /// </summary>
        ANDNOT = 2,
        /// <summary>
        /// Exclusive Or
        /// </summary>
        XOR = 3
    }
}
