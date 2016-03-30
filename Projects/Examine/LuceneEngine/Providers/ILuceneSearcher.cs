using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Providers
{
    /// <summary>
    /// An interface representing a lucene searcher with strongly typed results
    /// </summary>
    public interface ILuceneSearcher : ISearcher<ILuceneSearchResults, LuceneSearchResult, LuceneSearchCriteria>
    {
        Searcher GetSearcher();
    }
}