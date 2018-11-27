using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Provider;
using System.Linq;
using System.Security;
using Examine;
using System.Xml.Linq;
using Examine.LuceneEngine;

namespace Examine.Providers
{
    /// <summary>
    /// Base class for an Examine Index Provider. You must implement this class to create an IndexProvider
    /// </summary>
    public abstract class BaseIndexProvider : ProviderBase, IIndexer
    {
        private FieldDefinitionCollection _fieldDefinitionCollection;
        private readonly IEnumerable<FieldDefinition> _fieldDefinitions;
        private readonly string _name;

        /// <summary>
        /// Constructor for creating a provider/config based index
        /// </summary>
        protected BaseIndexProvider() { }

        /// <summary>
        /// Constructor for creating an indexer at runtime
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldDefinitions"></param>
        protected BaseIndexProvider(string name, IEnumerable<FieldDefinition> fieldDefinitions)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            _name = name;
            _fieldDefinitions = fieldDefinitions ?? throw new ArgumentNullException(nameof(fieldDefinitions));
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

        public override string Name => _name ?? base.Name;

        #region IIndexer members

        public abstract ISearcher GetSearcher();
        public abstract void IndexItems(IEnumerable<ValueSet> values);
        
        /// <summary>
        /// Deletes a node from the index
        /// </summary>
        /// <param name="nodeId">Node to delete</param>
        public abstract void DeleteFromIndex(string nodeId);

        /// <summary>
        /// Creates a new index, any existing index will be deleted
        /// </summary>
        public abstract void CreateIndex();
        
        /// <summary>
        /// Defines the mappings for field types to index field types
        /// </summary>
        /// <remarks>
        /// This is mutable but changes will only work prior to accessing the resolved value types
        /// </remarks>
        public FieldDefinitionCollection FieldDefinitionCollection => _fieldDefinitionCollection ?? (_fieldDefinitionCollection = new FieldDefinitionCollection(_fieldDefinitions));

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
        /// Raised before the item is indexed allowing developers to customize the data that get's stored in the index
        /// </summary>
        public event EventHandler<IndexingItemEventArgs> TransformingIndexValues;
        
        #endregion

        #region Protected Event callers
        
        /// <summary>
        /// Raises the <see cref="E:IndexingError"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Examine.IndexingErrorEventArgs"/> instance containing the event data.</param>
        protected virtual void OnIndexingError(IndexingErrorEventArgs e)
        {
            IndexingError?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:TransformingIndexValues"/> event.
        /// </summary>
        /// <param name="e">The <see cref="IndexingItemEventArgs"/> instance containing the event data.</param>
        protected virtual void OnTransformingIndexValues(IndexingItemEventArgs e)
        {
            TransformingIndexValues?.Invoke(this, e);
        }

        #endregion



    }
}
