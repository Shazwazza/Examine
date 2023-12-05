using System.Collections.Generic;
using Examine.Search;
using Lucene.Net.Facet;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Lucene DrillDown Query Dimensions
    /// </summary>
    public class LuceneDrillDownQueryDimensions : IDrillDownQueryDimensions
    {
        private readonly Dictionary<string, List<LuceneDrillDownQueryDimensionBase>> _dimensions = new Dictionary<string, List<LuceneDrillDownQueryDimensionBase>>();
        private readonly FacetsConfig _facetsConfig;

        public LuceneDrillDownQueryDimensions(FacetsConfig facetsConfig)
        {
            _facetsConfig = facetsConfig;
        }

        /// <inheritdoc/>
        public IDrillDownQueryDimensions AddDimension(string dimensionName, params string[] paths)
        {
            var dimension = new LuceneDrillDownQueryDimensionPath(dimensionName, paths);
            if (!_dimensions.TryGetValue(dimensionName, out var dims))
            {
                _dimensions[dimensionName] = new List<LuceneDrillDownQueryDimensionBase>();
            }
            _dimensions[dimensionName].Add(dimension);
            return this;
        }

        /// <summary>
        /// Add the dimension to the Drill-down Query
        /// </summary>
        /// <param name="drillDownQuery"></param>
        public void Apply(DrillDownQuery drillDownQuery)
        {
            foreach (var item in _dimensions)
            {
                foreach (var dim in item.Value)
                {
                    dim.Apply(drillDownQuery);
                }
            }
        }
    }
}
