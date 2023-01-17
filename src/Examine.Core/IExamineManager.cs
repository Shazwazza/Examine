using System.Collections.Generic;
using System.Linq;
using Examine.Suggest;

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
        /// Gets a list of all manually configured suggester providers
        /// </summary>
        /// <remarks>
        /// This returns only those suggesters explicitly registered with <see cref="AddSuggester"/> or config based suggesters
        /// </remarks>
        IEnumerable<ISuggester> RegisteredSuggesters { get; }

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

        /// <summary>
        /// Returns a sugesster that was registered with <see cref="AddSuggester"/> or via config
        /// </summary>
        /// <param name="suggesterName"></param>
        /// <param name="suggester"></param>
        /// <returns>
        /// true if the suggester was found by name
        /// </returns>
        bool TryGetSuggester(string suggesterName, out ISuggester suggester);

    }
}
