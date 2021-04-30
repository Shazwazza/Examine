using System;
using Examine.Search;

namespace Examine
{
    ///<summary>
    /// Abstract searcher
    ///</summary>
    public abstract class BaseSearchProvider : ISearcher
    {
        protected BaseSearchProvider(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            Name = name;
        }

        public string Name { get; }

        /// <summary>
        /// Searches the index
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public abstract ISearchResults Search(string searchText, QueryOptions options = null);

        /// <inheritdoc />
		public abstract IQuery CreateQuery(string category = null, BooleanOperation defaultOperation = BooleanOperation.And);
        
    }
}
