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
        private readonly IndexOptions _indexOptions;

        /// <summary>
        /// Constructor for creating an indexer at runtime
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="name"></param>
        /// <param name="indexOptions"></param>
        protected BaseIndexProvider(ILoggerFactory loggerFactory, string name,
            IOptionsMonitor<IndexOptions> indexOptions)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            LoggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<BaseIndexProvider>();
            Name = name;
            _indexOptions = indexOptions.GetNamedOptions(name);
        }

        /// <summary>
        /// Represents a type used to configure the logging system and create instances of
        /// <see cref="ILogger"/> from the registered Microsoft.Extensions.Logging.ILoggerProviders.
        /// </summary>
        protected ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// The index name
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        /// A validator to validate a value set before it's indexed
        /// </summary>
        public IValueSetValidator ValueSetValidator => _indexOptions.Validator;

        /// <summary>
        /// Ensures that the node being indexed is of a correct type
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected ValueSetValidationResult ValidateItem(ValueSet item)
            => ValueSetValidator?.Validate(item) ?? new ValueSetValidationResult(ValueSetValidationStatus.Valid, item);

        /// <summary>
        /// Indexes the items in the <see cref="ValueSet"/>
        /// </summary>
        /// <param name="values"></param>
        /// <param name="onComplete">
        /// Called by the implementor once the items have been indexed
        /// </param>
        /// <remarks>
        /// Items will have been validated at this stage
        /// </remarks>
        protected abstract void PerformIndexItems(IEnumerable<ValueSet> values, Action<IndexOperationEventArgs> onComplete);

        /// <summary>
        /// Deletes an index item by id
        /// </summary>
        /// <param name="itemIds"></param>
        /// <param name="onComplete">
        /// Called by the implementor once the items have been indexed
        /// </param>
        protected abstract void PerformDeleteFromIndex(IEnumerable<string> itemIds,
            Action<IndexOperationEventArgs> onComplete);

        #region IIndex members

        /// <summary>
        /// The default searcher of the index
        /// </summary>
        public abstract ISearcher Searcher { get; }

        /// <inheritdoc />
        /// <summary>
        /// Validates the items and calls <see cref="M:Examine.Providers.BaseIndexProvider.PerformIndexItems(System.Collections.Generic.IEnumerable{Examine.ValueSet})" />
        /// </summary>
        /// <param name="values"></param>
        public void IndexItems(IEnumerable<ValueSet> values)
            => PerformIndexItems(
                values
                    .Select(ValidateItem)
                    .Where(x => x.Status != ValueSetValidationStatus.Failed)
                    .Select(x => x.ValueSet), OnIndexOperationComplete);

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
        public ReadOnlyFieldDefinitionCollection FieldDefinitions =>
            _indexOptions.FieldDefinitions ?? new FieldDefinitionCollection();

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

        /// <summary>
        /// Run when a index operation completes
        /// </summary>
        /// <param name="e"></param>
        protected void OnIndexOperationComplete(IndexOperationEventArgs e) => IndexOperationComplete?.Invoke(this, e);

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
        protected virtual void OnTransformingIndexValues(IndexingItemEventArgs e) =>
            TransformingIndexValues?.Invoke(this, e);

        #endregion
    }
}
