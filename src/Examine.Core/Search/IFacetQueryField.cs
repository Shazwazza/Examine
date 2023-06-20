namespace Examine.Search
{
    /// <summary>
    /// Represents a facet fulltext query field
    /// </summary>
    public interface IFacetQueryField
    {
        /// <summary>
        /// Maximum number of terms to return
        /// </summary>
        IFacetQueryField MaxCount(int count);

        /// <summary>
        /// Path Hierarchy
        /// </summary>
        IFacetQueryField SetPath(string[] path);
    }
}
