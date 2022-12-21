namespace Examine.Search
{
    public interface IFacetFloatRangeQueryField : IFacetAppending, IQueryExecutor
    {
        /// <summary>
        /// Sets the field where the facet information will be read from
        /// </summary>
        IFacetFloatRangeQueryField FacetField(string fieldName);
    }
}
