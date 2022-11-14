namespace Examine.Lucene.Search
{
    public interface IFacetValue
    {
        /// <summary>
        /// The label of the facet value
        /// </summary>
        string Label { get; }

        /// <summary>
        /// The occurrence of the facet field
        /// </summary>
        float Value { get; }
    }
}
