using System;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    public class DateTimeType : IndexValueTypeBase, IIndexRangeValueType<DateTime>
    {
        public DateTools.Resolution Resolution { get; private set; }

        public DateTimeType(string fieldName, DateTools.Resolution resolution, bool store = true)
            : base(fieldName, store)
        {
            Resolution = resolution;
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            DateTime parsedVal;
            if (!TryConvert(value, out parsedVal))
                return;

            var val = DateToLong(parsedVal);

            doc.Add(new NumericField(FieldName, Store ? Field.Store.YES : Field.Store.NO, true).SetLongValue(val));
            doc.Add(new NumericField(LuceneIndexer.SortedFieldNamePrefix + FieldName, Field.Store.YES, true).SetLongValue(val));
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

        public override Query GetQuery(string query, Searcher searcher, IManagedQueryParameters parameters)
        {
            DateTime parsedVal;
            if (!TryConvert(query, out parsedVal))
                return null;

            return GetQuery(parsedVal, parsedVal);
        }

        public Query GetQuery(DateTime? lower, DateTime? upper, bool lowerInclusive = true, bool upperInclusive = true, IManagedQueryParameters parameters = null)
        {
            return NumericRangeQuery.NewLongRange(FieldName,
                lower != null ? DateToLong(lower.Value) : (long?)null,
                upper != null ? DateToLong(upper.Value) : (long?)null, lowerInclusive, upperInclusive);
        }
    }
}
