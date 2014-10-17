using Examine.LuceneEngine.SearchCriteria;

namespace Examine.LuceneEngine.Providers
{
    /// <summary>
    /// An interface representing a lucene searcher with strongly typed results
    /// </summary>
    public interface ILuceneSearcher : ISearcher<ILuceneSearchResults, LuceneSearchResult, LuceneSearchCriteria>
    {
        
    }
}