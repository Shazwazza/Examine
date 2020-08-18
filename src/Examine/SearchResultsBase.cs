using System.Collections;
using System.Collections.Generic;

namespace Examine
{
    public abstract class SearchResultsBase : ISearchResults
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

        /// <summary>
        /// Gets the total number of results for the search
        /// </summary>
        /// <value>The total items from the search.</value>
        public long TotalItemCount { get; internal set; }

        /// <summary>
        /// Gets the total number of documents while enumerating results
        /// </summary>
        //NOTE: This is totally retarded but it is required for medium trust as I cannot put this code inside the Skip method... wtf
        protected abstract int GetTotalDocs();

        /// <summary>
        /// Gets a result at a given index while enumerating results
        /// </summary>
        protected abstract ISearchResult GetSearchResult(int index);

        /// <summary>
        /// Skips to a particular point in the search results.
        /// </summary>
        /// <remarks>
        /// This allows for lazy loading of the results paging. We don't go into Lucene until we have to.
        /// </remarks>
        /// <param name="skip">The number of items in the results to skip.</param>
        /// <returns>A collection of the search results</returns>
        public virtual IEnumerable<ISearchResult> Skip(int skip)
        {
            for (int i = skip, x = GetTotalDocs(); i < x; i++)
            {
                if (Results.TryGetValue(i, out ISearchResult result) == false)
                {
                    result = GetSearchResult(i);

                    if (result == null)
                    {
                        continue;
                    }

                    Results.Add(i, result);
                }

                //using yield return means if the user breaks out we wont keep going
                //only load what we need to load!
                //and we'll get it from our cache, this means you can go
                //forward/ backwards without degrading performance
                yield return result;
            }
        }

        /// <summary>
        /// Gets the enumerator starting at position 0
        /// </summary>
        /// <returns>A collection of the search results</returns>
        public virtual IEnumerator<ISearchResult> GetEnumerator()
        {
            return Skip(0).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}