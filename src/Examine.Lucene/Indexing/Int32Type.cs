using Examine.Lucene.Providers;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    public class Int32Type : IndexFieldRangeValueType<int>
    {
        public Int32Type(string fieldName, ILoggerFactory logger, bool store = true)
            : base(fieldName, logger, store)
        {
        }

        /// <summary>
        /// Can be sorted by the normal field name
        /// </summary>
        public override string SortableFieldName => FieldName;

        protected override void AddSingleValue(Document doc, object value)
        {
            if (!TryConvert(value, out int parsedVal))
                return;

            doc.Add(new Int32Field(FieldName,parsedVal, Store ? Field.Store.YES : Field.Store.NO));;
        }

        public override Query GetQuery(string query)
        {
            return !TryConvert(query, out int parsedVal) ? null : GetQuery(parsedVal, parsedVal);
        }

        public override Query GetQuery(int? lower, int? upper, bool lowerInclusive = true, bool upperInclusive = true)
        {
            return NumericRangeQuery.NewInt32Range(FieldName,
                lower,
                upper, lowerInclusive, upperInclusive);
        }
    }
}
