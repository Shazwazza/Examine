using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Provider;
using System.Globalization;
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
    public abstract class BaseIndexProvider : ProviderBase, IExamineIndexer
    {
        private IEnumerable<FieldDefinition> _fieldDefinitions;
        private bool _fieldDefsInit = false;

        /// <summary>
        /// Gets the defined field definitions
        /// </summary>        
        public IEnumerable<FieldDefinition> FieldDefinitions
        {
            get
            {
                if (!_fieldDefsInit)
                {
                    _fieldDefsInit = true;
                    _fieldDefinitions = InitializeFieldDefinitions(_fieldDefinitions);
                }
                return _fieldDefinitions;
            }
        }

        /// <summary>
        /// Constructor used for provider instantiation
        /// </summary>
        protected BaseIndexProvider()
        {
            _fieldDefinitions = Enumerable.Empty<FieldDefinition>();
        }


        /// <summary>
        /// Constructor for creating an indexer at runtime
        /// </summary>
        /// <param name="fieldDefinitions"></param>
        protected BaseIndexProvider(IEnumerable<FieldDefinition> fieldDefinitions)
        {
            _fieldDefinitions = fieldDefinitions;

            //for legacy, all empty collections means it will be ignored
            IndexerData = new IndexCriteria(Enumerable.Empty<IIndexField>(), Enumerable.Empty<IIndexField>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), -1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseIndexProvider"/> class.
        /// </summary>
        /// <param name="indexerData">The indexer data.</param>
        [Obsolete("IIndexCriteria should no longer be used")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected BaseIndexProvider(IIndexCriteria indexerData)
        {
            _fieldDefinitions = Enumerable.Empty<FieldDefinition>();
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

        /// <summary>
        /// Used to initialize the field definition collection
        /// </summary>
        /// <param name="originalDefinitions">
        /// The field definitions passed in via the contructor
        /// </param>
        /// <returns>
        /// The resulting field definitions that will be used by the indexer
        /// </returns>
        /// <remarks>
        /// This will concat any legacy IndexerData into the FieldDefinitions since the Legacy IndexerData property is not used anywhere else.
        /// </remarks>
        protected virtual IEnumerable<FieldDefinition> InitializeFieldDefinitions(IEnumerable<FieldDefinition> originalDefinitions)
        {
            return IndexerData != null ? originalDefinitions.Union(IndexerData.ToFieldDefinitions()) : originalDefinitions;
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
            IndexItems(new[] {node.ToValueSet(category, node.ExamineNodeTypeAlias())});
        }

        /// <summary>
        /// Re-indexes an item
        /// </summary>
        /// <param name="nodes"></param>
        public abstract void IndexItems(IEnumerable<ValueSet> nodes);
        
        /// <summary>
        /// Deletes a node from the index
        /// </summary>
        /// <param name="nodeId"></param>
        public void DeleteFromIndex(long nodeId)
        {
            DeleteFromIndex(nodeId.ToString(CultureInfo.InvariantCulture));
        }

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
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IIndexCriteria IndexerData { get; set; }

        /// <summary>
        /// Check if the index exists
        /// </summary>
        /// <returns></returns>
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

        [Obsolete("Use the TransformIndexValues event instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler<IndexingNodeEventArgs> NodeIndexing;

        [Obsolete("Use the ItemIndexed event instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler<IndexedNodeEventArgs> NodeIndexed;

        [Obsolete("Use the ItemIndexed event instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler<IndexedNodesEventArgs> NodesIndexed;

        /// <summary>
        /// Occurs when the collection of nodes have been indexed
        /// </summary>
        public event EventHandler<IndexItemEventArgs> ItemIndexed;

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
        
        [Obsolete("Use the TransformIndexValues event instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler<IndexingFieldDataEventArgs> GatheringFieldData;
        
        [Obsolete("Use the IgnoringValueSet event instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler<IndexingNodeDataEventArgs> IgnoringNode;

        /// <summary>
        /// Occurs when validation of the value set fails
        /// </summary>
        public event EventHandler<IndexItemEventArgs> IgnoringValueSet;

        #endregion


        #region Protected Event callers

        protected virtual void OnItemIndexed(IndexItemEventArgs e)
        {
            if (ItemIndexed != null)
                ItemIndexed(this, e);

            //raise the legacy event
#pragma warning disable CS0618 // Type or member is obsolete
            OnNodeIndexed(new IndexedNodeEventArgs(e.IndexItem.ValueSet.Id));
            OnNodesIndexed(new IndexedNodesEventArgs(IndexerData, new[] { new IndexedNode { NodeId = Convert.ToInt32(e.IndexItem.ValueSet.Id), Type = e.IndexItem.IndexType} }));
#pragma warning restore CS0618 // Type or member is obsolete

        }

        protected virtual void OnIgnoringIndexItem(IndexItemEventArgs e)
        {
            if (IgnoringValueSet != null)
                IgnoringValueSet(this, e);

            //raise the legacy event
#pragma warning disable CS0618 // Type or member is obsolete
            OnIgnoringNode(new IndexingNodeDataEventArgs(e.IndexItem.ValueSet));
#pragma warning restore CS0618 // Type or member is obsolete

        }

        [Obsolete("Use OnIgnoringValueSet instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
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

        [Obsolete("Use OnItemIndexed instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnNodeIndexed(IndexedNodeEventArgs e)
        {
            if (NodeIndexed != null)
                NodeIndexed(this, e);
        }

        [Obsolete("Use the OnTransformIndexValues event instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
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

        [Obsolete("Use the OnTransformIndexValues event instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnGatheringNodeData(IndexingNodeDataEventArgs e)
        {
            if (GatheringNodeData != null)
                GatheringNodeData(this, e);
        }

        protected virtual void OnTransformingIndexValues(TransformingIndexDataEventArgs e)
        {
            if (TransformingIndexValues != null)
                TransformingIndexValues(this, e);

            if (!e.Cancel)
            {
                //legacy events
                var args = new IndexingNodeEventArgs(e.IndexItem.ValueSet);
                OnNodeIndexing(args);
                if (args.Cancel)
                {
                    e.Cancel = args.Cancel;
                }
            }
            
        }

        [Obsolete("Use the OnTransformIndexValues event instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnGatheringFieldData(IndexingFieldDataEventArgs e)
        {
            if (GatheringFieldData != null)
                GatheringFieldData(this, e);
        }

        [Obsolete("Use OnItemIndexed instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnNodesIndexed(IndexedNodesEventArgs e)
        {
            if (NodesIndexed != null)
                NodesIndexed(this, e);
        }

        #endregion



    }

   
}
