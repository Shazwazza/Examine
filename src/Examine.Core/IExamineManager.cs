using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Examine
{
    /// <summary>
    /// Exposes searchers and indexers
    /// </summary>
    public interface IExamineManager
    {
        /// <summary>
        /// Gets a list of all index providers
        /// </summary>
        /// <remarks>
        /// This returns all config based indexes and indexers registered in code
        /// </remarks>
        public IEnumerable<IIndex> Indexes { get; }

        /// <summary>
        /// Gets a list of all manually configured search providers
        /// </summary>
        /// <remarks>
        /// This returns only those searchers explicitly registered with AddExamineSearcher or config based searchers
        /// </remarks>
        public IEnumerable<ISearcher> RegisteredSearchers { get; }

        /// <summary>
        /// Disposes the <see cref="IExamineManager"/>
        /// </summary>
        public void Dispose();

        /// <summary>
        /// Returns an indexer by name
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="index"></param>
        /// <returns>true if the index was found by name</returns>
        public bool TryGetIndex(string indexName,
            [MaybeNullWhen(false)]
            out IIndex index);

        /// <summary>
        /// Returns a searcher that was registered with AddExamineSearcher or via config
        /// </summary>
        /// <param name="searcherName"></param>
        /// <param name="searcher"></param>
        /// <returns>
        /// true if the searcher was found by name
        /// </returns>
        public bool TryGetSearcher(string searcherName,
            [MaybeNullWhen(false)]
            out ISearcher searcher);

    }
}
