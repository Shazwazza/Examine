namespace Examine.Search
{
    /// <summary>
    /// Drill-down Query Dimensions
    /// </summary>
    public interface IDrillDownQueryDimensions
    {
        /// <summary>
        /// Adds one dimension of drill-downs.
        /// Repeated dimensions are OR'd with the previous contraints for the dimension.
        /// All demensions are AND'd againt each other and the base query.
        /// </summary>
        /// <param name="dimensionName">Dimension Name</param>
        /// <param name="paths">Facet Category Paths</param>
        /// <returns></returns>
        IDrillDownQueryDimensions AddDimension(string dimensionName, params string[] paths);
    }
}
