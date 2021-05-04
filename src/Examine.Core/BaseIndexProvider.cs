using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Examine
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for an Examine Index Provider
    /// </summary>
    public abstract class BaseIndexProvider : IIndex
    {
        private readonly ILogger<BaseIndexProvider> _logger;
        private readonly Lazy<IndexOptions> _indexOptions;

        /// <summary>
        /// Constructor for creating an indexer at runtime
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldDefinitions"></param>
        /// <param name="validator"></param>
        protected BaseIndexProvider(ILoggerFactory loggerFactory, string name, IOptionsSnapshot<IndexOptions> indexOptions)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            LoggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<BaseIndexProvider>();
            Name = name;
            _indexOptions = new Lazy<IndexOptions>(() => indexOptions.GetNamedOptions(name));
        }

        protected ILoggerFactory LoggerFactory { get; }
        public virtual string Name { get; }

        /// <summary>
        /// A validator to validate a value set before it's indexed
        /// </summary>
        public IValueSetValidator ValueSetValidator => _indexOptions.Value.Validator;

        /// <summary>
        /// Ensures that the node being indexed is of a correct type 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected ValueSetValidationResult ValidateItem(ValueSet item)
            => ValueSetValidator?.Validate(item) ?? ValueSetValidationResult.Valid;

        /// <summary>
        /// Indexes the items in the <see cref="ValueSet"/>
        /// </summary>
        /// <param name="op"></param>
        /// <param name="onComplete">
        /// Called by the implementor once the items have been indexed
        /// </param>
        /// <remarks>
        /// Items will have been validated at this stage
        /// </remarks>
        protected abstract void PerformIndexItems(IEnumerable<ValueSet> op, Action<IndexOperationEventArgs> onComplete);

        /// <summary>
        /// Deletes an index item by id
        /// </summary>
        /// <param name="itemIds"></param>
        /// <param name="onComplete">
        /// Called by the implementor once the items have been indexed
        /// </param>
        protected abstract void PerformDeleteFromIndex(IEnumerable<string> itemIds, Action<IndexOperationEventArgs> onComplete);

        #region IIndex members

        public abstract ISearcher Searcher { get; }

        /// <inheritdoc />
        /// <summary>
        /// Validates the items and calls <see cref="M:Examine.Providers.BaseIndexProvider.PerformIndexItems(System.Collections.Generic.IEnumerable{Examine.ValueSet})" />
        /// </summary>
        /// <param name="values"></param>
        public void IndexItems(IEnumerable<ValueSet> values)
            => PerformIndexItems(values.Where(x => ValidateItem(x) != ValueSetValidationResult.Failed), OnIndexOperationComplete);

        /// <inheritdoc />
        public void DeleteFromIndex(IEnumerable<string> itemIds)
            => PerformDeleteFromIndex(itemIds, OnIndexOperationComplete);

        /// <summary>
        /// Creates a new index, any existing index will be deleted
        /// </summary>
        public abstract void CreateIndex();

        /// <summary>
        /// Returns the mappings for field types to index field types
        /// </summary>
        public ReadOnlyFieldDefinitionCollection FieldDefinitions => _indexOptions.Value.FieldDefinitions ?? new FieldDefinitionCollection();

        /// <summary>
        /// Check if the index exists
        /// </summary>
        /// <returns></returns>
        public abstract bool IndexExists();

        #endregion

        #region Events

        /// <inheritdoc />
        public event EventHandler<IndexOperationEventArgs> IndexOperationComplete;

        /// <inheritdoc />
        public event EventHandler<IndexingErrorEventArgs> IndexingError;

        /// <inheritdoc />
        public event EventHandler<IndexingItemEventArgs> TransformingIndexValues;

        #endregion

        #region Protected Event callers

        private void OnIndexOperationComplete(IndexOperationEventArgs e) => IndexOperationComplete?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="E:IndexingError"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Examine.IndexingErrorEventArgs"/> instance containing the event data.</param>
        protected virtual void OnIndexingError(IndexingErrorEventArgs e)
        {
            _logger.LogError(e.Exception, e.Message);
            IndexingError?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:TransformingIndexValues"/> event.
        /// </summary>
        /// <param name="e">The <see cref="IndexingItemEventArgs"/> instance containing the event data.</param>
        protected virtual void OnTransformingIndexValues(IndexingItemEventArgs e) => TransformingIndexValues?.Invoke(this, e);

        #endregion



    }
}
