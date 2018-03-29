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
        void AddIndexer(string name, IIndexer indexer);

        /// <summary>
        /// Adds an index searcher to the manager - generally this would be a multi index searcher since most searchers are created from an existing index
        /// </summary>
        /// <param name="name"></param>
        /// <param name="searcher"></param>
        void AddSearcher(string name, ISearcher searcher);

        /// <summary>
        /// Deletes index for node for all providers
        /// </summary>
        /// <param name="nodeId"></param>
        void DeleteFromIndexes(string nodeId);

        /// <summary>
        /// Deletes index for node for the specified providers
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="providers"></param>
        void DeleteFromIndexes(string nodeId, IEnumerable<IIndexer> providers);

        void Dispose();

        /// <summary>
        /// Returns an indexer by name
        /// </summary>
        /// <param name="indexerName"></param>
        /// <returns></returns>
        IIndexer GetIndexer(string indexerName);

        /// <summary>
        /// Returns the searcher for a given index
        /// </summary>
        /// <param name="indexerName"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if no indexer was found with the provided name
        /// </exception>
        ISearcher GetSearcher(string indexerName);

        /// <summary>
        /// Returns a searcher that was registered with <see cref="AddSearcher"/>
        /// </summary>
        /// <param name="searcherName"></param>
        /// <returns>
        /// Returns null if not found, otherwise an <see cref="ISearcher"/> reference
        /// </returns>
        ISearcher GetRegisteredSearcher(string searcherName);

        /// <summary>
        /// Indexes all items for the index category for all providers
        /// </summary>
        /// <param name="indexCategory"></param>
        void IndexAll(string indexCategory);

        /// <summary>
        /// Reindex items for all providers
        /// </summary>
        /// <param name="nodes"></param>
        void IndexItems(ValueSet[] nodes);

        /// <summary>
        /// Re-indexes items for the providers specified
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="providers"></param>
        void IndexItems(ValueSet[] nodes, IEnumerable<IIndexer> providers);

        /// <summary>
        /// Rebuilds indexes for all providers
        /// </summary>
        void RebuildIndexes();
    }
}