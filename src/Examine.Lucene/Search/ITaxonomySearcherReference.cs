using Lucene.Net.Facet.Taxonomy.Directory;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents a Taxonomy Searcher Reference
    /// </summary>
    public interface ITaxonomySearcherReference : ISearcherReference
    {
        /// <summary>
        /// Taxonomy Reader for the sidecar taxonomy index
        /// </summary>
        DirectoryTaxonomyReader TaxonomyReader { get; }
    }
}
