using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Provider;
using System.Linq;
using System.Security;
using Examine;
using System.Xml.Linq;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine;

namespace Examine.Providers
{
    /// <summary>
    /// Base class for an Examine Index Provider. You must implement this class to create an IndexProvider
    /// </summary>
    public abstract class BaseIndexProvider : ProviderBase, IIndexer, IExamineIndexer
    {
        public IEnumerable<FieldDefinition> FieldDefinitions { get; private set; }
        
        /// <summary>
        /// Constructor used for provider instantiation
        /// </summary>
        protected BaseIndexProvider()
        {
            FieldDefinitions = Enumerable.Empty<FieldDefinition>();
        }


        /// <summary>
        /// Constructor for creating an indexer at runtime
        /// </summary>
        /// <param name="fieldDefinitions"></param>
        protected BaseIndexProvider(IEnumerable<FieldDefinition> fieldDefinitions)
        {
            FieldDefinitions = fieldDefinitions;

            //for legacy, all empty collections means it will be ignored
            IndexerData = new IndexCriteria(Enumerable.Empty<IIndexField>(), Enumerable.Empty<IIndexField>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), -1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseIndexProvider"/> class.
        /// </summary>
        /// <param name="indexerData">The indexer data.</param>
        [Obsolete("IIndexCriteria should no longer be used")]
        protected BaseIndexProvider(IIndexCriteria indexerData)
        {
            FieldDefinitions = Enumerable.Empty<FieldDefinition>();
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
        /// <param name="category">Type of index to use</param>
        [Obsolete("Use ValueSets with IndexItems instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ReIndexNode(XElement node, string category)
        {
            IndexItems(node.ToValueSet(category, node.ExamineNodeTypeAlias()));
        }

        /// <summary>
        /// Re-indexes an item
        /// </summary>
        /// <param name="nodes"></param>
        public abstract void IndexItems(params ValueSet[] nodes);
        
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
        [Obsolete("IIndexCriteria should no longer be used")]
        public IIndexCriteria IndexerData { get; set; }

        public abstract bool IndexExists();

        /// <summary>
        /// Returns true if the index is brand new/empty
        /// </summary>
        /// <returns></returns>
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
        [Obsolete("Use the TransformIndexValues event instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler<IndexingNodeDataEventArgs> GatheringNodeData;

        /// <summary>
        /// Raised before the item is indexed allowing developers to customize the data that get's stored in the index
        /// </summary>
        public event EventHandler<TransformingIndexDataEventArgs> TransformingIndexValues;

        /// <summary>
        /// Occurs when a node is deleted from the index
        /// </summary>
        public event EventHandler<DeleteIndexEventArgs> IndexDeleted;
        /// <summary>
        /// Occurs when a particular field is having its data obtained
        /// </summary>
        [Obsolete("Use the TransformIndexValues event instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
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

        protected virtual void OnTransformingIndexValues(TransformingIndexDataEventArgs e)
        {
            if (TransformingIndexValues != null)
                TransformingIndexValues(this, e);
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
