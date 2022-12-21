namespace Examine.Search
{
    /// <summary>
    /// Allows for appending more operations
    /// </summary>
    public interface IFacetAppending
    {
        /// <summary>
        /// Allows for adding more operations
        /// </summary>
        /// <returns></returns>
        IFaceting And();
    }
}
