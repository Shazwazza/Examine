using System;
using Examine.Lucene.Search;
using Examine.Lucene.Spatial.Search;
using Examine.Search;
using Lucene.Net.Documents;
using Lucene.Net.Queries.Function;
using Lucene.Net.Search;
using Lucene.Net.Spatial;
using Lucene.Net.Spatial.Queries;
using Microsoft.Extensions.Logging;
using Spatial4n.Distance;
using Spatial4n.Shapes;

namespace Examine.Lucene.Spatial.Indexing
{
    /// <summary>
    /// WKT Spatial Index Field Value Type
    /// </summary>
    public class WKTSpatialIndexFieldValueType : SpatialIndexFieldValueTypeBase
    {
        private readonly bool _stored;
        private Spatial4nShapeFactory _shapeFactory;

        /// <inheritdoc/>
        public override IExamineSpatialShapeFactory ExamineSpatialShapeFactory => _shapeFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="spatialStrategyFactory">Given field name, return Spatial Strategy</param>
        /// <param name="stored"></param>
        public WKTSpatialIndexFieldValueType(string fieldName, ILoggerFactory loggerFactory, Func<string, SpatialStrategy> spatialStrategyFactory, bool stored = true)
            : base(fieldName, loggerFactory, spatialStrategyFactory, true)
        {
            _stored = stored;
            _shapeFactory = new Spatial4nShapeFactory(SpatialStrategy.SpatialContext);
        }

        /// <inheritdoc/>
        protected override void AddSingleValue(Document doc, object value)
        {
            if (TryConvert<ExamineLuceneShape>(value, out var examineLuceneShape))
            {
                IShape shape = examineLuceneShape.Shape;
                foreach (Field field in SpatialStrategy.CreateIndexableFields(shape))
                {
                    doc.Add(field);
                }

                if (_stored)
                {
                    doc.Add(new StoredField(ExamineFieldNames.SpecialFieldPrefix + FieldName, SpatialStrategy.SpatialContext.ToString(shape)));
                }
            }
            else if (TryConvert<string>(value, out var str))
            {
                IShape shape = SpatialStrategy.SpatialContext.ReadShapeFromWkt(str);
                foreach (Field field in SpatialStrategy.CreateIndexableFields(shape))
                {
                    doc.Add(field);
                }

                if (_stored)
                {
                    doc.Add(new StoredField(ExamineFieldNames.SpecialFieldPrefix + FieldName, str));
                }
            }
        }

        /// <inheritdoc/>
        public override Query GetQuery(string query)
        {
            var spatialArgs = SpatialArgsParser.Parse(query, SpatialStrategy.SpatialContext);
            return SpatialStrategy.MakeQuery(spatialArgs);
        }

        /// <inheritdoc/>
        public override SortField ToSpatialDistanceSortField(SortableField sortableField, SortDirection sortDirection)
        {
            var pt = (sortableField.SpatialPoint as ExamineLucenePoint).Shape as IPoint;
            if (!SpatialStrategy.SpatialContext.IsGeo)
            {
                throw new NotSupportedException("This implementation may not be suitable for non GeoSpatial SpatialContext");
            }
            var valueSource = SpatialStrategy.MakeDistanceValueSource(pt, DistanceUtils.DegreesToKilometers);//the distance (in km)
            return(valueSource.GetSortField(sortDirection == SortDirection.Descending));
        }

        /// <inheritdoc/>
        public override Query GetQuery(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IExamineSpatialShape> shape)
        {
            var shapeVal = shape(ExamineSpatialShapeFactory);
            var luceneSpatialOperation = MapToSpatialOperation(spatialOperation);
            var spatial4nShape = (shapeVal as ExamineLuceneShape)?.Shape;
            var spatialArgs = new SpatialArgs(luceneSpatialOperation, spatial4nShape);
            var query = SpatialStrategy.MakeQuery(spatialArgs);
            return query;
        }

        /// <inheritdoc/>
        public override Filter GetFilter(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IExamineSpatialShape> shape)
        {
            var shapeVal = shape(ExamineSpatialShapeFactory);
            var luceneSpatialOperation = MapToSpatialOperation(spatialOperation);
            var spatial4nShape = (shapeVal as ExamineLuceneShape)?.Shape;
            var spatialArgs = new SpatialArgs(luceneSpatialOperation, spatial4nShape);
            var filter = SpatialStrategy.MakeFilter(spatialArgs);
            return filter;
        }

        private static SpatialOperation MapToSpatialOperation(ExamineSpatialOperation spatialOperation)
        {
            SpatialOperation luceneSpatialOperation;
            switch (spatialOperation)
            {
                case ExamineSpatialOperation.Intersects:
                    luceneSpatialOperation = SpatialOperation.Intersects;
                    break;
                case ExamineSpatialOperation.Overlaps:
                    luceneSpatialOperation = SpatialOperation.Overlaps;
                    break;
                case ExamineSpatialOperation.IsWithin:
                    luceneSpatialOperation = SpatialOperation.IsWithin;
                    break;
                case ExamineSpatialOperation.BoundingBoxIntersects:
                    luceneSpatialOperation = SpatialOperation.BBoxIntersects;
                    break;
                case ExamineSpatialOperation.BoundingBoxWithin:
                    luceneSpatialOperation = SpatialOperation.BBoxWithin;
                    break;
                case ExamineSpatialOperation.Contains:
                    luceneSpatialOperation = SpatialOperation.Contains;
                    break;
                case ExamineSpatialOperation.IsDisjointTo:
                    luceneSpatialOperation = SpatialOperation.IsDisjointTo;
                    break;
                case ExamineSpatialOperation.IsEqualTo:
                    luceneSpatialOperation = SpatialOperation.IsEqualTo;
                    break;
                default:
                    throw new NotSupportedException(nameof(spatialOperation));
            }

            return luceneSpatialOperation;
        }
    }
}
