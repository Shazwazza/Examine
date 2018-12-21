using Examine.LuceneEngine.Providers;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    public class Int32Type : IndexFieldValueTypeBase, IIndexRangeValueType<int>
    {
        public Int32Type(string fieldName, bool store = true)
            : base(fieldName, store)
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

            doc.Add(new NumericField(FieldName, Store ? Field.Store.YES : Field.Store.NO, true).SetIntValue(parsedVal));
        }

        public override Query GetQuery(string query, Searcher searcher)
        {
            return !TryConvert(query, out int parsedVal) ? null : GetQuery(parsedVal, parsedVal);
        }

        public Query GetQuery(int? lower, int? upper, bool lowerInclusive = true, bool upperInclusive = true)
        {
            return NumericRangeQuery.NewIntRange(FieldName,
                lower,
                upper, lowerInclusive, upperInclusive);
        }
    }
}
