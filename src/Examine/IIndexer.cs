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
        /// <param name="itemId">Node to delete</param>
        void DeleteFromIndex(string itemId);
        
        /// <summary>
        /// Re-indexes all data for the index type specified
        /// </summary>
        /// <param name="category"></param>
        void IndexAll(string category);

        /// <summary>
        /// Rebuilds the entire index from scratch for all index types
        /// </summary>
        void RebuildIndex();
        
        /// <summary>
        /// Returns the field definitions for the index
        /// </summary>
        FieldDefinitionCollection FieldDefinitionCollection { get; }

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
