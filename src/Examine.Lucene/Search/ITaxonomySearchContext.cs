namespace Examine.Lucene.Search
{
    public interface ITaxonomySearchContext : ISearchContext
    {
        ITaxonomySearcherReference GetTaxonomyAndSearcher();
    }
}
