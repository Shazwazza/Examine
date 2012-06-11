using System.Collections.Generic;
using System.Xml.Linq;

namespace Examine
{
    /// <summary>
    /// Interface to represent an Examine Indexer
    /// </summary>
    public interface IIndexer
    {
        //NOTE: this would be a better approach like in v2 but would be quite a breaking change.
        ///// <summary>
        ///// Reindexes many nodes at once
        ///// </summary>
        ///// <param name="nodes"></param>
        ///// <param name="type"></param>
        //void ReIndexNodes(IEnumerable<XElement> nodes, string type);

        /// <summary>
        /// Forces a particular XML node to be reindexed
        /// </summary>
        /// <param name="node">XML node to reindex</param>
        /// <param name="type">Type of index to use</param>
        void ReIndexNode(XElement node, string type);
        
        /// <summary>
        /// Deletes a node from the index
        /// </summary>
        /// <param name="nodeId">Node to delete</param>
        void DeleteFromIndex(string nodeId);
        
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
        /// Gets/sets the index criteria to create the index with
        /// </summary>
        /// <value>The indexer data.</value>
        IIndexCriteria IndexerData { get; set; }

        /// <summary>
        /// determines whether the index exsists or not
        /// </summary>
        bool IndexExists();

    }
}
