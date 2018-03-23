using Examine.LuceneEngine.Providers;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    /// <summary>
    /// Interface for parameters for managed queries (i.e. queries provided by IIndexValueType)
    /// </summary>
    public interface IManagedQueryParameters
    {
        object GetParameter(string name);
    }

    public interface IIndexRangeValueType<T> where T : struct
    {
        Query GetQuery(T? lower, T? upper, bool lowerInclusive = true, bool upperInclusive = true, IManagedQueryParameters parameters = null);
    }

    public class DoubleType : IndexValueTypeBase, IIndexRangeValueType<double>
    {
        public DoubleType(string fieldName, bool store= true)
            : base(fieldName, store)
        {
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            double parsedVal;
            if (!TryConvert(value, out parsedVal))
                return;

            doc.Add(new NumericField(FieldName, Store ? Field.Store.YES : Field.Store.NO, true).SetDoubleValue(parsedVal));
            doc.Add(new NumericField(LuceneIndexer.SortedFieldNamePrefix + FieldName, Field.Store.YES, true).SetDoubleValue(parsedVal));
        }

        public override Query GetQuery(string query, Searcher searcher, IManagedQueryParameters parameters)
        {
            double parsedVal;
            if (!TryConvert(query, out parsedVal))
                return null;

            return GetQuery(parsedVal, parsedVal);
        }

        public Query GetQuery(double? lower, double? upper, bool lowerInclusive = true, bool upperInclusive = true, IManagedQueryParameters parameters = null)
        {
            return NumericRangeQuery.NewDoubleRange(FieldName,
                lower ?? double.MinValue,
                upper ?? double.MaxValue, lowerInclusive, upperInclusive);
        }
    }
}
