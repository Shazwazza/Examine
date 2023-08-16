namespace Examine.Lucene.Search
{
    /// <summary>
    /// Search Context for Taxonomy Searcher
    /// </summary>
    public interface ITaxonomySearchContext : ISearchContext
    {
        /// <summary>
        /// Gets the Search and Taxonomny Reader reference
        /// </summary>
        /// <returns></returns>
        ITaxonomySearcherReference GetTaxonomyAndSearcher();
    }
}
