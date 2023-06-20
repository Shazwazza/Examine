using System;
using Examine.Search;

namespace Examine
{
    ///<summary>
    /// Abstract searcher
    ///</summary>
    public abstract class BaseSearchProvider : ISearcher
    {
        /// <inheritdoc/>
        protected BaseSearchProvider(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            Name = name;
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public abstract ISearchResults Search(string searchText, QueryOptions options = null);

        /// <inheritdoc />
		public abstract IQuery CreateQuery(string category = null, BooleanOperation defaultOperation = BooleanOperation.And);
        
    }
}
