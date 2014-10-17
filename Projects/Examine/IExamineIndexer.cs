using Examine.LuceneEngine.Indexing;

namespace Examine
{
    /// <summary>
    /// Interface for indexing
    /// </summary>
    public interface IExamineIndexer
    {
        /// <summary>
        /// Method to re-index specific data
        /// </summary>
        /// <param name="nodes"></param>
        void IndexItems(params ValueSet[] nodes);

        /// <summary>
        /// Deletes a node from the index
        /// </summary>
        /// <param name="nodeId">Node to delete</param>
        void DeleteFromIndex(long nodeId);

        /// <summary>
        /// Re-indexes all data for the index type specified
        /// </summary>
        /// <param name="type"></param>
        void IndexAll(string type);

        /// <summary>
        /// Rebuilds the entire index from scratch for all index types
        /// </summary>
        void RebuildIndex();

        /// <summary>
        /// determines whether the index exsists or not
        /// </summary>
        bool IndexExists();

        /// <summary>
        /// Determines if the index is new (contains any data)
        /// </summary>
        /// <returns></returns>
        bool IsIndexNew();
    }
}