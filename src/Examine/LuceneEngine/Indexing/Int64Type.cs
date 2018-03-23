using Examine.LuceneEngine.Providers;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    public class Int64Type : IndexValueTypeBase, IIndexRangeValueType<long>
    {
        public Int64Type(string fieldName, bool store = true)
            : base(fieldName, store)
        {
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            long parsedVal;
            if (!TryConvert(value, out parsedVal))
                return;

            doc.Add(new NumericField(FieldName, Store ? Field.Store.YES : Field.Store.NO, true).SetLongValue(parsedVal));
            doc.Add(new NumericField(LuceneIndexer.SortedFieldNamePrefix + FieldName, Field.Store.YES, true).SetLongValue(parsedVal));
        }

        public override Query GetQuery(string query, Searcher searcher, IManagedQueryParameters parameters)
        {
            long parsedVal;
            if (!TryConvert(query, out parsedVal))
                return null;

            return GetQuery(parsedVal, parsedVal);
        }

        public Query GetQuery(long? lower, long? upper, bool lowerInclusive = true, bool upperInclusive = true, IManagedQueryParameters parameters = null)
        {
            return NumericRangeQuery.NewLongRange(FieldName,
                lower != null ? lower.Value : (long?)null,
                upper != null ? upper.Value : (long?)null, lowerInclusive, upperInclusive);
        }
    }
}
