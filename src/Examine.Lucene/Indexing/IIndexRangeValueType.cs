using Lucene.Net.Search;

namespace Examine.Lucene.Indexing
{
    /// <summary>
    /// Used for value range types when the type requires parsing from string
    /// </summary>
    public interface IIndexRangeValueType
    {
        /// <summary>
        /// Gets a query as <see cref="Query"/>
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <param name="lowerInclusive"></param>
        /// <param name="upperInclusive"></param>
        /// <returns></returns>
        Query GetQuery(string lower, string upper, bool lowerInclusive = true, bool upperInclusive = true);
    }

    /// <summary>
    /// Used for value range types when the type is known
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IIndexRangeValueType<T> where T : struct
    {
        /// <summary>
        /// Gets a query as <see cref="Query"/>
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <param name="lowerInclusive"></param>
        /// <param name="upperInclusive"></param>
        /// <returns></returns>
        Query GetQuery(T? lower, T? upper, bool lowerInclusive = true, bool upperInclusive = true);
    }
}
