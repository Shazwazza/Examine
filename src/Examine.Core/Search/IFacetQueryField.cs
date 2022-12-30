namespace Examine.Search
{
    public interface IFacetQueryField
    {
        /// <summary>
        /// Maximum number of terms to return
        /// </summary>
        IFacetQueryField MaxCount(int count);

        /// <summary>
        /// Set the Facet Path
        /// </summary>
        /// <param name="path">Facet Path</param>
        IFacetQueryField Path(params string[] path);
    }
}
