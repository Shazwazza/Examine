using Lucene.Net.Spatial;
using System;
using Microsoft.Extensions.Logging;
using Lucene.Net.Spatial.Queries;
using Examine.Search;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Spatial.Prefix;
using Spatial4n.Context;
using Lucene.Net.Search;
using Examine.Lucene.Indexing;

namespace Examine.Lucene.Spatial.Indexing
{
    /// <summary>
    /// Spatial Index Field Value Type
    /// </summary>
    public abstract class SpatialIndexFieldValueTypeBase : IndexFieldValueTypeBase, ISpatialIndexFieldValueTypeBase
    {
        /// <summary>
        /// Spatial Strategy for Field
        /// </summary>
        public SpatialStrategy SpatialStrategy { get; }

        /// <summary>
        /// Spatial Args Parser for Field
        /// </summary>
        public SpatialArgsParser SpatialArgsParser { get; }


        /// <inheritdoc/>
        public abstract IExamineSpatialShapeFactory ExamineSpatialShapeFactory { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="spatialStrategyFactory"></param>
        /// <param name="store"></param>
        protected SpatialIndexFieldValueTypeBase(string fieldName, ILoggerFactory loggerFactory, Func<string, SpatialStrategy> spatialStrategyFactory, bool store = true)
            : base(fieldName, loggerFactory, store)
        {
            SpatialStrategy = spatialStrategyFactory(fieldName);
            SpatialArgsParser = new SpatialArgsParser();
        }

        /// <inheritdoc/>
        public abstract SortField ToSpatialDistanceSortField(SortableField sortableField, SortDirection sortDirection);

        /// <summary>
        /// Creates a RecursivePrefixTreeStrategy for A Geo SpatialContext
        /// </summary>
        /// <param name="maxLevels">Default value of 11 results in sub-meter precision for geohash</param>
        /// <returns>SpatialStrategy Factory</returns>
        public static Func<string, SpatialStrategy> GeoSpatialPrefixTreeStrategyFactory(int maxLevels = 11)
        {
            Func<string, SpatialStrategy> geoSpatialPrefixTreeStrategy = (fieldName) =>
            {
                var ctx = SpatialContext.Geo;

                SpatialPrefixTree grid = new GeohashPrefixTree(ctx, maxLevels);
                var strategy = new RecursivePrefixTreeStrategy(grid, fieldName);
                return strategy;
            };
            return geoSpatialPrefixTreeStrategy;
        }

        /// <inheritdoc/>
        public abstract Query GetQuery(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IExamineSpatialShape> shape);

        /// <inheritdoc/>
        public abstract Filter GetFilter(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IExamineSpatialShape> shape);
    }
}
