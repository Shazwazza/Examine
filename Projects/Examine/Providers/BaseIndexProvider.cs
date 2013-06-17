using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using Examine;
using System.Xml.Linq;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine;

namespace Examine.Providers
{
    /// <summary>
    /// Base class for an Examine Index Provider. You must implement this class to create an IndexProvider
    /// </summary>
    public abstract class BaseIndexProvider : ProviderBase, IIndexer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseIndexProvider"/> class.
        /// </summary>
        protected BaseIndexProvider() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseIndexProvider"/> class.
        /// </summary>
        /// <param name="indexerData">The indexer data.</param>
        protected BaseIndexProvider(IIndexCriteria indexerData)
        {
            IndexerData = indexerData;
        }

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The name of the provider is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The name of the provider has a length of zero.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
        /// </exception>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);
            
        }

        
        #region IIndexer members
        
        
        /// <summary>
        /// Forces a particular XML node to be reindexed
        /// </summary>
        /// <param name="node">XML node to reindex</param>
        /// <param name="type">Type of index to use</param>
        [Obsolete("Use ValueSets instead")]
        public virtual void ReIndexNode(XElement node, string type)
        {
            ReIndexNode(node.ToValueSet(type));
        }

        public abstract void ReIndexNode(ValueSet node);

        
        

        /// <summary>
        /// Deletes a node from the index
        /// </summary>
        /// <param name="nodeId">Node to delete</param>
        public abstract void DeleteFromIndex(string nodeId);

        /// <summary>
        /// Re-indexes all data for the index type specified
        /// </summary>
        /// <param name="type"></param>
        public abstract void IndexAll(string type);

        /// <summary>
        /// Rebuilds the entire index from scratch for all index types
        /// </summary>
        public abstract void RebuildIndex();

        /// <summary>
        /// Gets/sets the index criteria to create the index with
        /// </summary>
        public IIndexCriteria IndexerData { get; set; }

        public abstract bool IndexExists();
        public virtual bool IsIndexNew()
        {
            return !IndexExists();
        }

        #endregion

        #region Events
        /// <summary>
        /// Occurs for an Indexing Error
        /// </summary>
        public event EventHandler<IndexingErrorEventArgs> IndexingError;

        /// <summary>
        /// Occurs when a node is in its Indexing phase
        /// </summary>
        public event EventHandler<IndexingNodeEventArgs> NodeIndexing;
        /// <summary>
        /// Occurs when a node is in its Indexed phase
        /// </summary>
        public event EventHandler<IndexedNodeEventArgs> NodeIndexed;
        /// <summary>
        /// Occurs when a collection of nodes are in their Indexing phase (before a single node is processed)
        /// </summary>
        public event EventHandler<IndexingNodesEventArgs> NodesIndexing;
        /// <summary>
        /// Occurs when the collection of nodes have been indexed
        /// </summary>
        public event EventHandler<IndexedNodesEventArgs> NodesIndexed;

        /// <summary>
        /// Occurs when the indexer is gathering the fields and their associated data for the index
        /// </summary>
        [Obsolete("Use the TransformValues event instead")]
        public event EventHandler<IndexingNodeDataEventArgs> GatheringNodeData;
        /// <summary>
        /// Occurs when a node is deleted from the index
        /// </summary>
        public event EventHandler<DeleteIndexEventArgs> IndexDeleted;
        /// <summary>
        /// Occurs when a particular field is having its data obtained
        /// </summary>
        [Obsolete("Use the TransformValues event instead")]
        public event EventHandler<IndexingFieldDataEventArgs> GatheringFieldData;
        /// <summary>
        /// Occurs when node is found but outside the supported node set
        /// </summary>
        public event EventHandler<IndexingNodeDataEventArgs> IgnoringNode;

        

        #endregion
      

        #region Protected Event callers

        /// <summary>
        /// Called when a node is ignored by the ValidateDocument method.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnIgnoringNode(IndexingNodeDataEventArgs e)
        {
            if (IgnoringNode != null)
                IgnoringNode(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:IndexingError"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Examine.IndexingErrorEventArgs"/> instance containing the event data.</param>
        protected virtual void OnIndexingError(IndexingErrorEventArgs e)
        {
            if (IndexingError != null)
                IndexingError(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:NodeIndexed"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Examine.IndexedNodeEventArgs"/> instance containing the event data.</param>
        protected virtual void OnNodeIndexed(IndexedNodeEventArgs e)
        {
            if (NodeIndexed != null)
                NodeIndexed(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:NodeIndexing"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Examine.IndexingNodeEventArgs"/> instance containing the event data.</param>
        protected virtual void OnNodeIndexing(IndexingNodeEventArgs e)
        {
            if (NodeIndexing != null)
                NodeIndexing(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:IndexDeleted"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Examine.DeleteIndexEventArgs"/> instance containing the event data.</param>
        protected virtual void OnIndexDeleted(DeleteIndexEventArgs e)
        {
            if (IndexDeleted != null)
                IndexDeleted(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:GatheringNodeData"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Examine.IndexingNodeDataEventArgs"/> instance containing the event data.</param>
        protected virtual void OnGatheringNodeData(IndexingNodeDataEventArgs e)
        {
            if (GatheringNodeData != null)
                GatheringNodeData(this, e);
        }


        
        /// <summary>
        /// Raises the <see cref="E:GatheringFieldData"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Examine.IndexingFieldDataEventArgs"/> instance containing the event data.</param>
        protected virtual void OnGatheringFieldData(IndexingFieldDataEventArgs e)
        {
            if (GatheringFieldData != null)
                GatheringFieldData(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:NodesIndexed"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Examine.IndexedNodesEventArgs"/> instance containing the event data.</param>
        protected virtual void OnNodesIndexed(IndexedNodesEventArgs e)
        {
            if (NodesIndexed != null)
                NodesIndexed(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:NodesIndexing"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Examine.IndexingNodesEventArgs"/> instance containing the event data.</param>
        protected virtual void OnNodesIndexing(IndexingNodesEventArgs e)
        {
            if (NodesIndexing != null)
                NodesIndexing(this, e);
        }

        #endregion



    }

   
}
