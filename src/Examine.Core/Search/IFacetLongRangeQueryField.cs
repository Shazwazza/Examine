namespace Examine.Search
{
    public interface IFacetLongRangeQueryField : IFacetAppending, IQueryExecutor
    {
        /// <summary>
        /// Sets the field where the facet information will be read from
        /// </summary>
        IFacetLongRangeQueryField FacetField(string fieldName);
    }
}
