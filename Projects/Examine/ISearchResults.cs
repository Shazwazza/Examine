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
    public interface ISearchResults : ISearchResults<ISearchResult>
    {
        
    }

    internal class SearchResultsProxy<T> : ISearchResults
        where T : ISearchResult
    {
        private readonly ISearchResults<T> _realResults;

        public SearchResultsProxy(ISearchResults<T> realResults)
        {
            _realResults = realResults;
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
    }
}
