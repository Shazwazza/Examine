using System;
using System.Collections.Generic;

namespace Examine
{

    /// <summary>
    /// Interface to represent an Examine Indexer
    /// </summary>
    public interface IIndex
    {
        /// <summary>
        /// The index name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns a searcher for the index
        /// </summary>
        /// <returns></returns>
        ISearcher Searcher { get; }

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
        ReadOnlyFieldDefinitionCollection FieldDefinitions { get; }

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

        /// <summary>
        /// Occurs for an Indexing Error
        /// </summary>
        event EventHandler<IndexingErrorEventArgs> IndexingError;
    }
}
