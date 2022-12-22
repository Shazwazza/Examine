namespace Examine.Search
{
    public interface IFacetField
    {
        /// <summary>
        /// The field name
        /// </summary>
        string Field { get; }

        /// <summary>
        /// The field to get the facet field from
        /// </summary>
        string FacetField { get; }

    }
}
