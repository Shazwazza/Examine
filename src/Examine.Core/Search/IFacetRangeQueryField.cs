namespace Examine.Search
{
    public interface IFacetRangeQueryField : IBooleanOperation
    {
        /// <summary>
        /// Sets the field where the facet information will be read from
        /// </summary>
        IFacetRangeQueryField FacetField(string fieldName);
    }
}
