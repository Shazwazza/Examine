namespace Examine.Search
{
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
