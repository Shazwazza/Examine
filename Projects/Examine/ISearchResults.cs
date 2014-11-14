using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.SearchCriteria;

namespace Examine
{
    public interface ISearchResults<out T> : IEnumerable<T>
        where T : ISearchResult
    {
        int TotalItemCount { get; }

        IEnumerable<T> Skip(int skip);

        
    }

    //NOTE: This is just for backwards compatibility
    public interface ISearchResults : IEnumerable<SearchResult>
    {
        int TotalItemCount { get; }
        IEnumerable<SearchResult> Skip(int skip);
    }

    internal class SearchResultsProxy<T> : ISearchResults
        where T : ISearchResult
    {
        private readonly ISearchResults<T> _realResults;

        public SearchResultsProxy(ISearchResults<T> realResults)
        {
            _realResults = realResults;
        }

        //This is for backwards compatibility
        IEnumerator<SearchResult> IEnumerable<SearchResult>.GetEnumerator()
        {
            return _realResults.OfType<SearchResult>().GetEnumerator();
        }

        public IEnumerator<ISearchResult> GetEnumerator()
        {
            return _realResults.Cast<ISearchResult>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int TotalItemCount
        {
            get { return _realResults.TotalItemCount; }
        }

        public IEnumerable<ISearchResult> Skip(int skip)
        {
            return _realResults.Skip(skip).Cast<ISearchResult>();
        }

        IEnumerable<SearchResult> ISearchResults.Skip(int skip)
        {
            return _realResults.OfType<SearchResult>().Skip(skip);
        }
    }
}
