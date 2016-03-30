using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Lucene.Net.Analysis;

namespace Examine
{
    public interface ISearchableLuceneExamineIndex : ISearchableExamineIndex<ILuceneSearchResults, LuceneSearchResult, LuceneSearchCriteria>
    {
        new ILuceneSearcher GetSearcher();
        ILuceneSearcher GetSearcher(Analyzer searchAnalyzer);
    }

    public interface ISearchableExamineIndex<out TResults, TResult, out TSearchCriteria>
        where TResults : ISearchResults<TResult>
        where TResult : ISearchResult
        where TSearchCriteria : ISearchCriteria
    {
        /// <summary>
        /// Returns a searcher for the indexer
        /// </summary>
        /// <typeparam name="TResults"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TSearchCriteria"></typeparam>
        /// <returns></returns>
        ISearcher<TResults, TResult, TSearchCriteria> GetSearcher();
    }
}