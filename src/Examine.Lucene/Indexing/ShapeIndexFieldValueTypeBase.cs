using Lucene.Net.Spatial;
using System;
using Microsoft.Extensions.Logging;
using Lucene.Net.Spatial.Queries;
using Examine.Search;
using Examine.Lucene.Search;

namespace Examine.Lucene.Indexing
{
    public abstract class ShapeIndexFieldValueTypeBase : IndexFieldValueTypeBase
    {
        private readonly SpatialStrategy _spatialStrategy;
        private readonly SpatialArgsParser _spatialArgsParser;

        public SpatialStrategy SpatialStrategy => _spatialStrategy;

        public SpatialArgsParser SpatialArgsParser => _spatialArgsParser;

        public abstract IExamineSpatialShapeFactory ExamineSpatialShapeFactory { get; }

        protected ShapeIndexFieldValueTypeBase(string fieldName, ILoggerFactory loggerFactory, Func<string, SpatialStrategy> spatialStrategyFactory, bool store = true)
            : base(fieldName, loggerFactory, store)
        {
            _spatialStrategy = spatialStrategyFactory(fieldName);
            _spatialArgsParser = new SpatialArgsParser();
        }
    }
}
