using Examine.Lucene.Providers;
using Lucene.Net.Documents;
using Lucene.Net.Facet.SortedSet;
using Lucene.Net.Search;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    public class Int64Type : IndexFieldRangeValueType<long>
    {
        private readonly bool _isFacetable;

        public Int64Type(string fieldName, ILoggerFactory logger, bool store = true, bool isFacetable = false)
            : base(fieldName, logger, store)
        {
            _isFacetable = isFacetable;
        }

        /// <summary>
        /// Can be sorted by the normal field name
        /// </summary>
        public override string SortableFieldName => FieldName;

        protected override void AddSingleValue(Document doc, object value)
        {
            if (!TryConvert(value, out long parsedVal))
                return;

            doc.Add(new Int64Field(FieldName,parsedVal, Store ? Field.Store.YES : Field.Store.NO));

            if (_isFacetable)
            {
                doc.Add(new SortedSetDocValuesFacetField(FieldName, parsedVal.ToString()));
                doc.Add(new NumericDocValuesField(FieldName, parsedVal));
            }
        }

        public override Query GetQuery(string query)
        {
            return !TryConvert(query, out long parsedVal) ? null : GetQuery(parsedVal, parsedVal);
        }

        public override Query GetQuery(long? lower, long? upper, bool lowerInclusive = true, bool upperInclusive = true)
        {
            return NumericRangeQuery.NewInt64Range(FieldName,
                lower,
                upper, lowerInclusive, upperInclusive);
        }
    }
}
