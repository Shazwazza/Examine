using System.Collections.Generic;
using System.Xml.Linq;
using Examine.Providers;

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
        /// <param name="itemId">Node to delete</param>
        void DeleteFromIndex(string itemId);
        
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
        
    }
}
