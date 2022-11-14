namespace Examine.Search
{
    public interface IFacetQueryField : IBooleanOperation
    {
        /// <summary>
        /// Maximum number of terms to return
        /// </summary>
        IFacetQueryField MaxCount(int count);

        /// <summary>
        /// Sets the field where the facet information will be read from
        /// </summary>
        IFacetQueryField FacetField(string fieldName);
    }
}
