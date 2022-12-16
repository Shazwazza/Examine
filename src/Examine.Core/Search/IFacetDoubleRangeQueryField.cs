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

        /// <summary>
        /// Sets the field where the facet information will be read from
        /// </summary>
        IFacetDoubleRangeQueryField FacetField(string fieldName);
    }
}
