namespace Examine.Search
{
    public interface IFacetDoubleRangeQueryField : IFacetAppending, IQueryExecutor
    {
        /// <summary>
        /// Sets if the range query is on <see cref="float"/> values
        /// </summary>
        /// <param name="isFloat"></param>
        /// <returns></returns>
        IFacetDoubleRangeQueryField IsFloat(bool isFloat);
    }
}
