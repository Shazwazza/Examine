using System;
using System.Configuration.Provider;
using Examine;
using System.Xml.Linq;

namespace Examine.Providers
{
    public abstract class BaseIndexProvider : ProviderBase, IIndexer
    {

        public BaseIndexProvider() { }
        public BaseIndexProvider(IIndexCriteria indexerData)
        {
            IndexerData = indexerData;
        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);
            if (config["enabled"] == null)
                throw new ArgumentNullException("enabled flag on index provider has not been set");

            bool enabled;
            if (!bool.TryParse(config["enabled"], out enabled))
                throw new ArgumentNullException("enabled flag on index provider has not been set");

            Enabled = enabled;
        }

        public bool Enabled { get; set; }

        #region IIndexer members
        /// <summary>
        /// Determines if the manager will call the indexing methods when content is saved or deleted as
        /// opposed to cache being updated.
        /// </summary>
        public abstract bool SupportUnpublishedContent { get; protected set; }
        public abstract void ReIndexNode(XElement node, IndexType type);
        public abstract void DeleteFromIndex(XElement node);
        public abstract void IndexAll(IndexType type);
        public abstract void RebuildIndex();
        /// <summary>
        /// Gets/sets the index criteria to create the index with
        /// </summary>
        public IIndexCriteria IndexerData { get; set; }
        #endregion

        #region Events
        public event EventHandler<IndexingErrorEventArgs> IndexingError;

        public event EventHandler<IndexingNodeEventArgs> NodeIndexing;
        public event EventHandler<IndexedNodeEventArgs> NodeIndexed;
        public event EventHandler<IndexingNodesEventArgs> NodesIndexing;
        public event EventHandler<IndexedNodesEventArgs> NodesIndexed;

        public event EventHandler<IndexingNodeDataEventArgs> GatheringNodeData;
        public event EventHandler<DeleteIndexEventArgs> IndexDeleted;
        public event EventHandler<IndexingFieldDataEventArgs> GatheringFieldData;
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

        protected virtual void OnIndexingError(IndexingErrorEventArgs e)
        {
            if (IndexingError != null)
                IndexingError(this, e);
        }

        protected virtual void OnNodeIndexed(IndexedNodeEventArgs e)
        {
            if (NodeIndexed != null)
                NodeIndexed(this, e);
        }

        protected virtual void OnNodeIndexing(IndexingNodeEventArgs e)
        {
            if (NodeIndexing != null)
                NodeIndexing(this, e);
        }

        protected virtual void OnIndexDeleted(DeleteIndexEventArgs e)
        {
            if (IndexDeleted != null)
                IndexDeleted(this, e);
        }

        protected virtual void OnGatheringNodeData(IndexingNodeDataEventArgs e)
        {
            if (GatheringNodeData != null)
                GatheringNodeData(this, e);
        }

        protected virtual void OnGatheringFieldData(IndexingFieldDataEventArgs e)
        {
            if (GatheringFieldData != null)
                GatheringFieldData(this, e);
        }

        protected virtual void OnNodesIndexed(IndexedNodesEventArgs e)
        {
            if (NodesIndexed != null)
                NodesIndexed(this, e);
        }

        protected virtual void OnNodesIndexing(IndexingNodesEventArgs e)
        {
            if (NodesIndexing != null)
                NodesIndexing(this, e);
        }

        #endregion



    }
}
