using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Examine.Lucene.Indexing;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;

namespace Examine.Lucene
{
    /// <summary>
    /// Maintains a collection of field names names and their <see cref="IIndexFieldValueType"/> for an index
    /// </summary>
    public class FieldValueTypeCollection
    {
        /// <summary>
        /// Returns the PerFieldAnalyzerWrapper
        /// </summary>
        public PerFieldAnalyzerWrapper Analyzer { get; }

        /// <summary>
        /// Create a <see cref="FieldValueTypeCollection"/>
        /// </summary>
        /// <param name="analyzer">The default <see cref="Analyzer"/> to use for the resulting <see cref="PerFieldAnalyzerWrapper"/> used for indexing</param>
        /// <param name="valueTypeFactories">List of value type factories to initialize the collection with</param>
        /// <param name="fieldDefinitionCollection"></param>
        public FieldValueTypeCollection(
            Analyzer defaultAnalyzer,
            IReadOnlyDictionary<string, IFieldValueTypeFactory> valueTypeFactories,
            ReadOnlyFieldDefinitionCollection fieldDefinitionCollection)
        {
            ValueTypeFactories = new ValueTypeFactoryCollection();

            foreach (KeyValuePair<string, IFieldValueTypeFactory> type in valueTypeFactories)
            {
                ValueTypeFactories.TryAdd(type.Key, type.Value);
            }

            var fieldAnalyzers = new Dictionary<string, Analyzer>();

            //initializes the collection of field aliases to it's correct IIndexFieldValueType
            _resolvedValueTypes = new Lazy<ConcurrentDictionary<string, IIndexFieldValueType>>(() =>
            {
                var result = new ConcurrentDictionary<string, IIndexFieldValueType>();

                foreach (FieldDefinition field in fieldDefinitionCollection)
                {
                    if (!string.IsNullOrWhiteSpace(field.Type) && ValueTypeFactories.TryGetFactory(field.Type, out var valueTypeFactory))
                    {
                        IIndexFieldValueType valueType = valueTypeFactory.Create(field.Name);
                        fieldAnalyzers.Add(field.Name, valueType.Analyzer);
                        result.TryAdd(valueType.FieldName, valueType);
                    }
                    else
                    {
                        //Define the default!
                        if (!ValueTypeFactories.TryGetFactory(FieldDefinitionTypes.FullText, out var fullText))
                        {
                            throw new InvalidOperationException($"The value type factory {FieldDefinitionTypes.FullText} was not found");
                        }

                        IIndexFieldValueType valueType = fullText.Create(field.Name);
                        fieldAnalyzers.Add(field.Name, valueType.Analyzer);
                        result.TryAdd(valueType.FieldName, valueType);
                    }
                }
                return result;
            });

            Analyzer = new PerFieldAnalyzerWrapper(defaultAnalyzer, fieldAnalyzers);
        }

        /// <summary>
        /// Returns the <see cref="IIndexFieldValueType"/> for the field name specified
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValueTypeFactory"></param>
        /// <returns></returns>
        /// <remarks>
        /// If it's not found it will create one with the factory supplied and initialize it.
        /// </remarks>
        public IIndexFieldValueType GetValueType(string fieldName, IFieldValueTypeFactory fieldValueTypeFactory)
            => _resolvedValueTypes.Value.GetOrAdd(fieldName, n =>
                {
                    IIndexFieldValueType t = fieldValueTypeFactory.Create(n);
                    return t;
                });

        /// <summary>
        /// Returns the value type for the field name specified
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// Throws an exception if a value type was not found
        /// </exception>
        public IIndexFieldValueType GetValueType(string fieldName)
        {
            if (!_resolvedValueTypes.Value.TryGetValue(fieldName, out IIndexFieldValueType valueType))
            {
                throw new InvalidOperationException($"No {nameof(IIndexFieldValueType)} was found for field name {fieldName}");
            }

            return valueType;
        }


        /// <summary>
        /// Defines the field types such as number, fulltext, etc...
        /// </summary>
        /// <remarks>
        /// This collection is mutable but must be changed before the EnsureIndex method is fired (i.e. on startup)
        /// </remarks>
        public ValueTypeFactoryCollection ValueTypeFactories { get; }

        private readonly Lazy<ConcurrentDictionary<string, IIndexFieldValueType>> _resolvedValueTypes;
        private readonly Analyzer _defaultAnalyzer;

        /// <summary>
        /// Returns the resolved collection of <see cref="IIndexFieldValueType"/> for this index
        /// </summary>
        public IEnumerable<IIndexFieldValueType> ValueTypes => _resolvedValueTypes.Value.Values;

    }
}
