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
    public abstract class SpatialIndexFieldValueTypeBase : IndexFieldValueTypeBase, ISpatialIndexFieldValueTypeBase
    {
        private readonly SpatialStrategy _spatialStrategy;
        private readonly SpatialArgsParser _spatialArgsParser;

        public SpatialStrategy SpatialStrategy => _spatialStrategy;

        public SpatialArgsParser SpatialArgsParser => _spatialArgsParser;

        public abstract IExamineSpatialShapeFactory ExamineSpatialShapeFactory { get; }

        protected SpatialIndexFieldValueTypeBase(string fieldName, ILoggerFactory loggerFactory, Func<string, SpatialStrategy> spatialStrategyFactory, bool store = true)
            : base(fieldName, loggerFactory, store)
        {
            _spatialStrategy = spatialStrategyFactory(fieldName);
            _spatialArgsParser = new SpatialArgsParser();
        }

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
                SpatialContext ctx = SpatialContext.Geo;

                SpatialPrefixTree grid = new GeohashPrefixTree(ctx, maxLevels);
                var strategy = new RecursivePrefixTreeStrategy(grid, fieldName);
                return strategy;
            };
            return geoSpatialPrefixTreeStrategy;
        }

        public abstract Query GetQuery(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IExamineSpatialShape> shape);
    }
}
