using System.Collections.Generic;
using System.Xml.Linq;
using Examine.Providers;

namespace Examine
{
    /// <summary>
    /// Interface to represent an Examine Indexer
    /// </summary>
    public interface IIndexer
    {
        /// <summary>
        /// Returns a searcher for the index
        /// </summary>
        /// <returns></returns>
        ISearcher GetSearcher();

        /// <summary>
        /// Method to re-index specific data
        /// </summary>
        /// <param name="values"></param>
        void IndexItems(IEnumerable<ValueSet> values);
        
        /// <summary>
        /// Deletes a node from the index
        /// </summary>
        /// <param name="nodeId">Node to delete</param>
        void DeleteFromIndex(string nodeId);
        
        /// <summary>
        /// Re-indexes all data for the index type specified
        /// </summary>
        /// <param name="category"></param>
        void IndexAll(string category);

        /// <summary>
        /// Rebuilds the entire index from scratch for all index types
        /// </summary>
        void RebuildIndex();

        ///// <summary>
        ///// Gets/sets the index criteria to create the index with
        ///// </summary>
        ///// <value>The indexer data.</value>
        //IIndexCriteria IndexerData { get; set; }

        IndexFieldDefinitions IndexFieldDefinitions { get; }

        /// <summary>
        /// determines whether the index exsists or not
        /// </summary>
        bool IndexExists();

    }
}
