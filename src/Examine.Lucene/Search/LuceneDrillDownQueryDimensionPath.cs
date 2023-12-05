using Lucene.Net.Facet;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Drill-down Query Dimension + Path
    /// </summary>
    public class LuceneDrillDownQueryDimensionPath : LuceneDrillDownQueryDimensionBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dimensionName">Dimension Name</param>
        /// <param name="paths">Facet Category Paths</param>
        public LuceneDrillDownQueryDimensionPath(string dimensionName, params string[] paths) : base(dimensionName)
        {
            Paths = paths;
        }

        /// <summary>
        /// Facet Category Paths
        /// </summary>
        public string[] Paths { get; }

        /// <inheritdoc/>
        public override void Apply(DrillDownQuery drillDownQuery) => drillDownQuery.Add(DimensionName, Paths);
    }
}
