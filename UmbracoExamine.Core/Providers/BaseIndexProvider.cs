using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;
using System.IO;
using UmbracoExamine.Core;
using umbraco.cms.businesslogic;

namespace UmbracoExamine.Providers
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
        public abstract void ReIndexNode(Content node, IndexType type);
        public abstract void DeleteFromIndex(Content node);
        public abstract void IndexAll(IndexType type);
        public abstract void RebuildIndex();
        /// <summary>
        /// Gets/sets the index criteria to create the index with
        /// </summary>
        public IIndexCriteria IndexerData { get; set; }
        #endregion

        #region Delegates
        public delegate void IndexingNodeEventHandler(object sender, IndexingNodeEventArgs e);
        public delegate void IndexingErrorEventHandler(object sender, IndexingErrorEventArgs e);

        /// <summary>
        /// Returns the value of what will be indexed. Event subscribers should return e.FieldValue if they wish not to modify it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public delegate string IndexingFieldDataEventHandler(object sender, IndexingFieldDataEventArgs e);

        /// <summary>
        /// Returns the full dictionary of what will be indexed. Event subscribers should return e.Value if they wish not to modify it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public delegate Dictionary<string, string> IndexingNodeDataEventHandler(object sender, IndexingNodeDataEventArgs e);

        /// <summary>
        /// Returns the xpath statement to select the umbraco nodes that will be indexed. Event subscribers should return e.XPath if they wish not to modify it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public delegate string IndexingNodesEventHandler(object sender, IndexingNodesEventArgs e);
        #endregion

        #region Events
        
        public event IndexingNodeEventHandler NodeIndexed;

        public event IndexingNodesEventHandler NodesIndexing;
        public event IndexingNodesEventHandler NodesIndexed;

        public event IndexingNodeDataEventHandler GatheringNodeData;
        public event IndexingNodeEventHandler NodeIndexDeleted;
        public event IndexingFieldDataEventHandler GatheringFieldData;
        public event IndexingErrorEventHandler IndexingError;
        public event IndexingNodeDataEventHandler IgnoringNode;
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

        protected virtual void OnNodeIndexed(IndexingNodeEventArgs e)
        {
            if (NodeIndexed != null)
                NodeIndexed(this, e);
        }

        protected virtual void OnNodeIndexDeleted(IndexingNodeEventArgs e)
        {
            if (NodeIndexDeleted != null)
                NodeIndexDeleted(this, e);
        }

        protected virtual Dictionary<string, string> OnGatheringNodeData(IndexingNodeDataEventArgs e)
        {
            if (GatheringNodeData != null)
                return GatheringNodeData(this, e);

            return e.Values;
        }

        protected virtual string OnGatheringFieldData(IndexingFieldDataEventArgs e)
        {
            if (GatheringFieldData != null)
                return GatheringFieldData(this, e);

            return e.FieldValue;
        }

        protected virtual string OnNodesIndexed(IndexingNodesEventArgs e)
        {
            if (NodesIndexed != null)
                NodesIndexed(this, e);

            return e.XPath;
        }

        protected virtual string OnNodesIndexing(IndexingNodesEventArgs e)
        {

            if (NodesIndexing != null)
                NodesIndexing(this, e);

            return e.XPath;
        }

        #endregion

        

    }
}
