namespace Examine.LuceneEngine.Faceting
{
    /// <summary>
    /// This is applied for the collection of FacetLevel for each document in a reader to ensure e.g. uniqueness of facet IDs.
    /// </summary>
    internal interface IFacetComb
    {
        FacetLevel[] Comb(FacetLevel[] levels);
    }
}
