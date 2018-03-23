using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Security;
using Examine;
using System.Xml.Linq;

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

        public abstract ISearcher GetSearcher();
        public abstract void IndexItems(IEnumerable<ValueSet> values);

        ///// <summary>
        ///// Forces a particular XML node to be reindexed
        ///// </summary>
        ///// <param name="node">XML node to reindex</param>
        ///// <param name="type">Type of index to use</param>
        //public abstract void ReIndexNode(XElement node, string type);

        /// <summary>
        /// Deletes a node from the index
        /// </summary>
        /// <param name="nodeId">Node to delete</param>
        public abstract void DeleteFromIndex(string nodeId);

        /// <summary>
        /// Re-indexes all data for the index type specified
        /// </summary>
        /// <param name="category"></param>
        public abstract void IndexAll(string category);

        /// <summary>
        /// Rebuilds the entire index from scratch for all index types
        /// </summary>
        public abstract void RebuildIndex();

        /// <summary>
        /// Gets/sets the index criteria to create the index with
        /// </summary>
        public IIndexCriteria IndexerData
        {
            get => _indexerData;
            set
            {
                _indexerData = value;
                //reset the combined data 
                _indexFieldDefinitions = null;
            }
        }

        private IndexFieldDefinitions _indexFieldDefinitions;
        private IIndexCriteria _indexerData;

        /// <summary>
        /// Defines the mappings for field types to index field types
        /// </summary>
        /// <remarks>
        /// This is mutable
        /// </remarks>
        public IndexFieldDefinitions IndexFieldDefinitions => _indexFieldDefinitions ?? (_indexFieldDefinitions = new IndexFieldDefinitions(IndexerData == null ? Enumerable.Empty<IIndexField>() : IndexerData.UserFields.Concat(IndexerData.StandardFields.ToList())));

        /// <summary>
        /// Check if the index exists
        /// </summary>
        /// <returns></returns>
        public abstract bool IndexExists();

        #endregion

        #region Events
        /// <summary>
        /// Occurs for an Indexing Error
        /// </summary>
        public event EventHandler<IndexingErrorEventArgs> IndexingError;

        /// <summary>
        /// Occurs when the indexer is gathering the fields and their associated data for the index
        /// </summary>
        public event EventHandler<IndexingItemEventArgs> GatheringNodeData;
        
        #endregion

        #region Protected Event callers
        
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
        /// Raises the <see cref="E:GatheringNodeData"/> event.
        /// </summary>
        /// <param name="e">The <see cref="IndexingItemDataEventArgs"/> instance containing the event data.</param>
        protected virtual void OnGatheringNodeData(IndexingItemEventArgs e)
        {
            if (GatheringNodeData != null)
                GatheringNodeData(this, e);
        }

        #endregion



    }
}
