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
        IReadOnlyDictionary<string, IIndex> IndexProviders { get; }

        /// <summary>
        /// Adds an indexer to the manager
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        void AddIndex(IIndex index);

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
        /// <param name="indexName"></param>
        /// <returns></returns>
        IIndex GetIndex(string indexName);
        
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