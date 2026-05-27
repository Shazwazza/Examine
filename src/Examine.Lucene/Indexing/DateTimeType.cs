using System;
using Examine.Lucene.Providers;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{

    public class DateTimeType : IndexFieldRangeValueType<DateTime>
    {
        public DateResolution Resolution { get; }

        /// <summary>
        /// Can be sorted by the normal field name
        /// </summary>
        public override string SortableFieldName => FieldName;

        public DateTimeType(string fieldName, ILoggerFactory logger, DateResolution resolution, bool store = true)
            : base(fieldName, logger, store)
        {
            Resolution = resolution;
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            if (!TryConvert(value, out DateTime parsedVal))
                return;

            var val = DateToLong(parsedVal);

            doc.Add(new Int64Field(FieldName,val, Store ? Field.Store.YES : Field.Store.NO));;
        }

        /// <summary>
        /// Returns the ticks to be indexed, then use NumericRangeQuery to query against it
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        protected long DateToLong(DateTime date)
        {
            return DateTools.Round(date, Resolution).Ticks;
        }

        public override Query GetQuery(string query)
        {
            if (!TryConvert(query, out DateTime parsedVal))
                return null;

            return GetQuery(parsedVal, parsedVal);
        }

        public override Query GetQuery(DateTime? lower, DateTime? upper, bool lowerInclusive = true, bool upperInclusive = true)
        {
            return NumericRangeQuery.NewInt64Range(FieldName,
                lower != null ? DateToLong(lower.Value) : (long?)null,
                upper != null ? DateToLong(upper.Value) : (long?)null, lowerInclusive, upperInclusive);
        }
    }
}
