using System.Collections.Generic;

namespace Examine
{
    public static class ExamineManagerExtensions
    {
        /// <summary>
        /// Returns the searcher for a given index
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="indexerName"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if no indexer was found with the provided name
        /// </exception>
        public static ISearcher GetSearcher(this IExamineManager manager, string indexerName)
        {
            if (manager.IndexProviders.ContainsKey(indexerName))
            {
                if (manager.IndexProviders.TryGetValue(indexerName, out var indexer))
                {
                    return indexer.GetSearcher();
                }
            }
            throw new KeyNotFoundException("No indexer defined by name " + indexerName);
        }
    }
}