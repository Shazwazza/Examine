namespace Examine.Lucene.Search
{
    public interface ILuceneSearchResults : ISearchResults
    {
        SearchAfterOptions SearchAfter { get; }
    }
}
