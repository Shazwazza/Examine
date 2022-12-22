using Lucene.Net.Facet.Taxonomy.Directory;

namespace Examine.Lucene.Search
{
    public interface ITaxonomySearcherReference : ISearcherReference
    {
        DirectoryTaxonomyReader TaxonomyReader { get; }
    }
}
