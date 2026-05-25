using Lucene.Net.Search;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    /// <summary>
    /// Used for value range types when the type is known
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class IndexFieldRangeValueType<T> : IndexFieldValueTypeBase, IIndexRangeValueType<T>, IIndexRangeValueType
         where T : struct
    {
        /// <inheritdoc/>
        protected IndexFieldRangeValueType(string fieldName, ILoggerFactory logger, bool store = true) : base(fieldName, logger, store)
        {
        }

        /// <inheritdoc/>
        public abstract Query GetQuery(T? lower, T? upper, bool lowerInclusive = true, bool upperInclusive = true);

        /// <inheritdoc/>
        public Query GetQuery(string lower, string upper, bool lowerInclusive = true, bool upperInclusive = true)
        {
            var lowerParsed = TryConvert<T>(lower, out var lowerValue);
            var upperParsed = TryConvert<T>(upper, out var upperValue);

            return GetQuery(lowerParsed ? (T?)lowerValue : null, upperParsed ? (T?)upperValue : null, lowerInclusive, upperInclusive);
        }
    }
}
