using System;
using System.Collections.Generic;
using System.Linq;

namespace Examine.Providers
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for an Examine Index Provider
    /// </summary>
    public abstract class BaseIndexProvider : IIndex
    {
        /// <summary>
        /// Constructor for creating an indexer at runtime
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldDefinitions"></param>
        /// <param name="validator"></param>
        protected BaseIndexProvider(string name, FieldDefinitionCollection fieldDefinitions, IValueSetValidator validator)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            Name = name;
            FieldDefinitionCollection = fieldDefinitions ?? throw new ArgumentNullException(nameof(fieldDefinitions));
            ValueSetValidator = validator;
        }

        public virtual string Name { get; }

        /// <summary>
        /// A validator to validate a value set before it's indexed
        /// </summary>
        public IValueSetValidator ValueSetValidator { get; protected set; }

        /// <summary>
        /// Ensures that the node being indexed is of a correct type 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected ValueSetValidationResult ValidateItem(ValueSet item)
        {
            return ValueSetValidator?.Validate(item) ?? ValueSetValidationResult.Valid;
        }

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

        public abstract ISearcher GetSearcher();

        /// <inheritdoc />
        /// <summary>
        /// Validates the items and calls <see cref="M:Examine.Providers.BaseIndexProvider.PerformIndexItems(System.Collections.Generic.IEnumerable{Examine.ValueSet})" />
        /// </summary>
        /// <param name="values"></param>
        public void IndexItems(IEnumerable<ValueSet> values)
        {
            PerformIndexItems(values.Where(x => ValidateItem(x) != ValueSetValidationResult.Failed), OnIndexOperationComplete);
        }

        /// <inheritdoc />
        public void DeleteFromIndex(IEnumerable<string> itemIds)
        {
            PerformDeleteFromIndex(itemIds, OnIndexOperationComplete);
        }

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
        public FieldDefinitionCollection FieldDefinitionCollection { get; }

        /// <summary>
        /// Check if the index exists
        /// </summary>
        /// <returns></returns>
        public abstract bool IndexExists();

        public abstract int GetCommitCount();

        #endregion

        #region Events

        /// <inheritdoc />
        public event EventHandler<IndexOperationEventArgs> IndexOperationComplete;

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

        private void OnIndexOperationComplete(IndexOperationEventArgs e)
        {
            IndexOperationComplete?.Invoke(this, e);
        }

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
