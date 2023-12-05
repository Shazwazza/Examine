namespace Examine.Search
{
    /// <summary>
    /// Drill Sideways Options
    /// </summary>
    public interface IDrillSideways
    {
        /// <summary>
        /// Set the number of Top Documents
        /// </summary>
        /// <param name="topN">Number of Top Documents</param>
        /// <returns></returns>
        IDrillSideways SetTopN(int topN);
    }
}
