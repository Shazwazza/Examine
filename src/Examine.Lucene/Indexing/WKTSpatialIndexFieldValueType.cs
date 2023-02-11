using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Spatial;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Queries;
using Microsoft.Extensions.Logging;
using Spatial4n.Context;
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
            if (TryConvert<string>(value, out var str))
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
    }
}
