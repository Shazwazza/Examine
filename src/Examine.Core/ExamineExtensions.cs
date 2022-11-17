using System;
using Microsoft.Extensions.Options;

namespace Examine
{
    /// <summary>
    /// Static methods to help query umbraco xml
    /// </summary>
    public static class ExamineExtensions
    {
        /// <summary>
        /// Gets named options from an <see cref="IOptionsMonitor{TOptions}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionsMonitor"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T GetNamedOptions<T>(this IOptionsMonitor<T> optionsMonitor, string name)
            where T : class
        {
            T options = optionsMonitor.Get(name);

            if (options == null)
            {
                throw new InvalidOperationException($"No named {typeof(T)} options with name {name}");
            }

            return options;
        }

        /// <summary>
        /// Gets the index by name, throw <see cref="InvalidOperationException"/> if not found
        /// </summary>
        /// <param name="examineManager"></param>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public static IIndex GetIndex(this IExamineManager examineManager, string indexName)
        {
            if (examineManager.TryGetIndex(indexName, out IIndex index))
            {
                return index;
            }
            throw new InvalidOperationException("No index found with name " + indexName);
        }

        /// <summary>
        /// Deletes a node from the index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="itemId"></param>
        public static void DeleteFromIndex(this IIndex index, string itemId)
        {
            index.DeleteFromIndex(new[] {itemId});
        }

        /// <summary>
        /// Method to re-index specific data
        /// </summary>
        /// <param name="index"></param>
        /// <param name="node"></param>
        public static void IndexItem(this IIndex index, ValueSet node)
        {
            index.IndexItems(new[] { node });
        }

    }
}
