using System;
using System.Collections.Generic;
using Examine.Lucene.Providers;
using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;
using Lucene.Net.Search;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{

    public class DateTimeType : IndexFieldRangeValueType<DateTime>, IIndexFacetValueType
    {
        public DateResolution Resolution { get; }

        private readonly bool _isFacetable;

        /// <summary>
        /// Can be sorted by the normal field name
        /// </summary>
        public override string SortableFieldName => FieldName;

        public DateTimeType(string fieldName, ILoggerFactory logger, DateResolution resolution, bool store, bool isFacetable)
            : base(fieldName, logger, store)
        {
            Resolution = resolution;
            _isFacetable = isFacetable;
        }

        public DateTimeType(string fieldName, ILoggerFactory logger, DateResolution resolution, bool store = true)
            : base(fieldName, logger, store)
        {
            Resolution = resolution;
            _isFacetable = false;
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            if (!TryConvert(value, out DateTime parsedVal))
                return;

            var val = DateToLong(parsedVal);

            doc.Add(new Int64Field(FieldName,val, Store ? Field.Store.YES : Field.Store.NO));

            if (_isFacetable)
            {
                doc.Add(new SortedSetDocValuesFacetField(FieldName, val.ToString()));
                doc.Add(new NumericDocValuesField(FieldName, val));
            }
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

        public virtual IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(FacetsCollector facetsCollector, SortedSetDocValuesReaderState sortedSetReaderState, IFacetField field)
            => field.ExtractFacets(facetsCollector, sortedSetReaderState);
    }
}
