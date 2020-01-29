using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Examine
{

    /// <summary>
    /// Interface to represent an Examine Indexer
    /// </summary>
    public interface IIndex
    {
        string Name { get; }

        /// <summary>
        /// Returns a searcher for the index
        /// </summary>
        /// <returns></returns>
        ISearcher GetSearcher();

        /// <summary>
        /// Method to index data
        /// </summary>
        /// <param name="values"></param>
        void IndexItems(IEnumerable<ValueSet> values);
        
        /// <summary>
        /// Deletes a node from the index
        /// </summary>
        /// <param name="itemIds">Node to delete</param>
        void DeleteFromIndex(IEnumerable<string> itemIds);
        
        /// <summary>
        /// Creates a new index, any existing index will be deleted
        /// </summary>
        void CreateIndex();
        
        /// <summary>
        /// Returns the field definitions for the index
        /// </summary>
        FieldDefinitionCollection FieldDefinitionCollection { get; }

        /// <summary>
        /// determines whether the index exsists or not
        /// </summary>
        bool IndexExists();

        /// <summary>
        /// Raised once an index operation is completed
        /// </summary>
        event EventHandler<IndexOperationEventArgs> IndexOperationComplete;

        /// <summary>
        /// Raised before the item is indexed allowing developers to customize the data that get's stored in the index
        /// </summary>
        event EventHandler<IndexingItemEventArgs> TransformingIndexValues;

    }
}
