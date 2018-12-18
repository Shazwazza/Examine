using Examine.Search;

namespace Examine.Providers
{
    ///<summary>
    /// Abstract search provider object
    ///</summary>
    public abstract class BaseSearchProvider : ISearcher
    {
        public abstract string Name { get; }

        /// <summary>
        /// Searches the index
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public abstract ISearchResults Search(string searchText, int maxResults = 500);

        /// <inheritdoc />
		public abstract IQuery CreateQuery(string type = null, BooleanOperation defaultOperation = BooleanOperation.And);
        
    }
}
