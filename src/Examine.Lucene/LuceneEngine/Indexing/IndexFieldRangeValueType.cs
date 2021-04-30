using Lucene.Net.Search;
using Microsoft.Extensions.Logging;

namespace Examine.LuceneEngine.Indexing
{
    public abstract class IndexFieldRangeValueType<T> : IndexFieldValueTypeBase, IIndexRangeValueType<T>, IIndexRangeValueType
         where T : struct
    {
        protected IndexFieldRangeValueType(string fieldName, ILogger<IndexFieldRangeValueType<T>> logger, bool store = true) : base(fieldName, logger, store)
        {
        }

        public abstract Query GetQuery(T? lower, T? upper, bool lowerInclusive = true, bool upperInclusive = true);

        public Query GetQuery(string lower, string upper, bool lowerInclusive = true, bool upperInclusive = true)
        {
            var lowerParsed = TryConvert<T>(lower, out var lowerValue);
            var upperParsed = TryConvert<T>(upper, out var upperValue);

            return GetQuery(lowerParsed ? (T?)lowerValue : null, upperParsed ? (T?)upperValue : null, lowerInclusive, upperInclusive);
        }
    }
}
