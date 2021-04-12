using System.Collections.Generic;
using System.Linq;

namespace Examine
{
    public interface IExamineManager
    {
        /// <summary>
        /// Gets a list of all index providers
        /// </summary>
        /// <remarks>
        /// This returns all config based indexes and indexers registered in code
        /// </remarks>
        IEnumerable<IIndex> Indexes { get; }

        /// <summary>
        /// Gets a list of all manually configured search providers
        /// </summary>
        /// <remarks>
        /// This returns only those searchers explicitly registered with <see cref="AddSearcher"/> or config based searchers
        /// </remarks>
        IEnumerable<ISearcher> RegisteredSearchers { get; }

        /// <summary>
        /// Adds an indexer to the manager
        /// </summary>
        /// <param name="index"></param>
        IIndex AddIndex(IIndex index);

        /// <summary>
        /// Adds an index searcher to the manager - generally this would be a multi index searcher since most searchers are created from an existing index
        /// </summary>
        /// <param name="searcher"></param>
        ISearcher AddSearcher(ISearcher searcher);

        void Dispose();

        /// <summary>
        /// Returns an indexer by name
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="index"></param>
        /// <returns>true if the index was found by name</returns>
        bool TryGetIndex(string indexName, out IIndex index);

        /// <summary>
        /// Returns a searcher that was registered with <see cref="AddSearcher"/> or via config
        /// </summary>
        /// <param name="searcherName"></param>
        /// <param name="searcher"></param>
        /// <returns>
        /// true if the searcher was found by name
        /// </returns>
        bool TryGetSearcher(string searcherName, out ISearcher searcher);

    }
}