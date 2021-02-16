using System;
using System.Collections;
using System.Collections.Generic;

namespace Examine
{
    public abstract class SearchResultsBase : ISearchResults2
    {
        public SearchResultsBase()
        {
            Results = new Dictionary<int, ISearchResult>();
        }

        ///<summary>
        /// Returns an empty search result
        ///</summary>
        ///<returns></returns>
        public static ISearchResults Empty()
        {
            return EmptySearchResults.Instance;
        }

        /// <summary>
        /// Results for the search
        /// </summary>
        protected IDictionary<int, ISearchResult> Results { get; }

        private long? _totalItemCount;

        /// <summary>
        /// Gets the total number of results for the search
        /// </summary>
        /// <value>The total items from the search.</value>
        public long TotalItemCount
        {
            get
            {
                // NOTE: Below is pretty ugly but to avoid breaking changes this is how the lazy execution of 
                // the search will work for now. 

                // TODO: How do we ensure that GetTotalDocs() is executed?
                if (!_totalItemCount.HasValue)
                {
                    _totalItemCount = 0; // initialize so we don't get into a loop

                    // ensure search, this will set the value of this property
                    GetTotalDocs();
                }
                return _totalItemCount ?? 0;
            }
            protected set
            {
                _totalItemCount = value;
            }
        }

        /// <summary>
        /// Executes the search and gets the total number of documents for enumerating the results
        /// </summary>
        protected abstract int GetTotalDocs();

        /// <summary>
        /// Executes the search and gets the total number of documents for enumerating the results
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        protected virtual int GetTotalDocs(int skip, int? take = null)
        {
            if (skip == 0 && take == null) 
                return GetTotalDocs();

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a result at a given index while enumerating results
        /// </summary>
        protected abstract ISearchResult GetSearchResult(int index);

        /// <inheritdoc />
        public virtual IEnumerable<ISearchResult> Skip(int skip)
        {
            for (int i = skip, x = GetTotalDocs(); i < x; i++)
            {
                var result = ProcessResult(i);
                
                if (result == null) 
                    continue;

                yield return result;
            }
        }

        /// <inheritdoc />
        public virtual IEnumerable<ISearchResult> SkipTake(int skip, int? take = null)
        {
            for (int i = 0, x = GetTotalDocs(skip, take); i < x; i++)
            {
                var result = ProcessResult(i);

                if (result == null)
                    continue;

                yield return result;
            }
        }

        private ISearchResult ProcessResult(int i)
        {
            if (Results.TryGetValue(i, out ISearchResult result) == false)
            {
                result = GetSearchResult(i);

                if (result == null)
                {
                    return null;
                }

                Results.Add(i, result);
            }
            return result;
        }

        /// <summary>
        /// Gets the enumerator starting at position 0
        /// </summary>
        /// <returns>A collection of the search results</returns>
        public virtual IEnumerator<ISearchResult> GetEnumerator() => Skip(0).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        
    }
}