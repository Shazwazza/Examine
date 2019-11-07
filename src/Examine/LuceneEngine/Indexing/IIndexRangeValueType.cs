using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    /// <summary>
    /// Used for value range types when the type requires parsing from string
    /// </summary>
    public interface IIndexRangeValueType
    {
        Query GetQuery(string lower, string upper, bool lowerInclusive = true, bool upperInclusive = true);
    }

    /// <summary>
    /// Used for value range types when the type is known
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IIndexRangeValueType<T> where T : struct
    {
        Query GetQuery(T? lower, T? upper, bool lowerInclusive = true, bool upperInclusive = true);
    }
}