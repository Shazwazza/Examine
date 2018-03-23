using Examine.LuceneEngine.Providers;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    public class Int32Type : IndexValueTypeBase, IIndexRangeValueType<int>
    {
        public Int32Type(string fieldName, bool store = true)
            : base(fieldName, store)
        {
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            int parsedVal;
            if (!TryConvert(value, out parsedVal))
                return;

            doc.Add(new NumericField(FieldName, Store ? Field.Store.YES : Field.Store.NO, true).SetIntValue(parsedVal));
            doc.Add(new NumericField(LuceneIndexer.SortedFieldNamePrefix + FieldName, Field.Store.YES, true).SetIntValue(parsedVal));
        }

        public override Query GetQuery(string query, Searcher searcher)
        {
            int parsedVal;
            if (!TryConvert(query, out parsedVal))
                return null;

            return GetQuery(parsedVal, parsedVal);
        }

        public Query GetQuery(int? lower, int? upper, bool lowerInclusive = true, bool upperInclusive = true)
        {
            return NumericRangeQuery.NewIntRange(FieldName,
                lower != null ? lower.Value : (int?)null,
                upper != null ? upper.Value : (int?)null, lowerInclusive, upperInclusive);
        }
    }
}
