using System;
using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.Documents;
using Lucene.Net.Queries.Function;
using Lucene.Net.Search;
using Lucene.Net.Spatial;
using Lucene.Net.Spatial.Queries;
using Microsoft.Extensions.Logging;
using Spatial4n.Distance;
using Spatial4n.Shapes;

namespace Examine.Lucene.Indexing
{
    public class WKTSpatialIndexFieldValueType : SpatialIndexFieldValueTypeBase
    {
        private readonly bool _stored;
        private Spatial4nShapeFactory _shapeFactory;
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
        public override Query GetQuery(string query)
        {
            var spatialArgs = SpatialArgsParser.Parse(query, SpatialStrategy.SpatialContext);
            return SpatialStrategy.MakeQuery(spatialArgs);
        }

        public override SortField ToSpatialDistanceSortField(SortableField sortableField, SortDirection sortDirection)
        {
            IPoint pt = (sortableField.SpatialPoint as ExamineLucenePoint).Shape as IPoint;
            //Reconsider line below as won't work in non geo
            ValueSource valueSource = SpatialStrategy.MakeDistanceValueSource(pt, DistanceUtils.DegreesToKilometers);//the distance (in km)
            return(valueSource.GetSortField(sortDirection == SortDirection.Descending));
        }
    }
}
