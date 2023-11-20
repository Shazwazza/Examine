using Lucene.Net.Facet;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Lucene DrillDown Query Dimensions base
    /// </summary>
    public abstract class LuceneDrillDownQueryDimensionBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dimensionName">Dimension Name</param>
        public LuceneDrillDownQueryDimensionBase(string dimensionName)
        {
            DimensionName = dimensionName;
        }

        /// <summary>
        /// Dimension Name
        /// </summary>
        public string DimensionName { get; }

        /// <summary>
        /// Add the dimension to the Drill-down Query
        /// </summary>
        /// <param name="drillDownQuery"></param>
        public abstract void Apply(DrillDownQuery drillDownQuery);
    }
}
