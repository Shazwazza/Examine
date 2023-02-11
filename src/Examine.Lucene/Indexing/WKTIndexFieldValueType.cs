using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Documents;
using Lucene.Net.Spatial;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    public class WKTIndexFieldValueType : IndexFieldValueTypeBase
    {
        private readonly SpatialStrategy _spatialStrategy;
        private readonly bool _collection;
        private readonly bool _stored;

        public WKTIndexFieldValueType(string fieldName, ILoggerFactory loggerFactory, SpatialStrategy spatialStrategy, bool collection = false, bool stored = true) : base(fieldName, loggerFactory, true)
        {
            _spatialStrategy = spatialStrategy;
            _collection = collection;
            _stored = stored;
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            if (TryConvert<string>(value, out var str))
            {

                if (_stored)
                {
                    doc.Add(new StoredField(FieldName, str));
                }
            }
        }
    }
}
