using System.Collections.Generic;

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
        IReadOnlyDictionary<string, IIndexer> IndexProviders { get; }

        /// <summary>
        /// Adds an indexer to the manager
        /// </summary>
        /// <param name="name"></param>
        /// <param name="indexer"></param>
        void AddIndexer(IIndexer indexer);

        /// <summary>
        /// Adds an index searcher to the manager - generally this would be a multi index searcher since most searchers are created from an existing index
        /// </summary>
        /// <param name="name"></param>
        /// <param name="searcher"></param>
        void AddSearcher(ISearcher searcher);

        void Dispose();

        /// <summary>
        /// Returns an indexer by name
        /// </summary>
        /// <param name="indexerName"></param>
        /// <returns></returns>
        IIndexer GetIndexer(string indexerName);
        
        /// <summary>
        /// Returns a searcher that was registered with <see cref="AddSearcher"/> or via config
        /// </summary>
        /// <param name="searcherName"></param>
        /// <returns>
        /// Returns null if not found, otherwise an <see cref="ISearcher"/> reference
        /// </returns>
        ISearcher GetSearcher(string searcherName);

    }
}