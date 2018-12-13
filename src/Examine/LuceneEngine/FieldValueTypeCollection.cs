using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Analysis;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// Maintains a collection of field names names and their <see cref="IIndexValueType"/> for an index
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
        /// <param name="analyzer">The <see cref="PerFieldAnalyzerWrapper"/> used for indexing</param>
        /// <param name="valueTypeFactories">List of value type factories to initialize the collection with</param>
        /// <param name="fieldDefinitionCollection"></param>
        public FieldValueTypeCollection(
            PerFieldAnalyzerWrapper analyzer, 
            IReadOnlyDictionary<string, IFieldValueTypeFactory> valueTypeFactories, 
            FieldDefinitionCollection fieldDefinitionCollection)
        {
            Analyzer = analyzer;
            foreach (var type in valueTypeFactories)
            {
                ValueTypeFactories.TryAdd(type.Key, type.Value);
            }

            //initializes the collection of field aliases to it's correct IIndexValueType
            _resolvedValueTypes = new Lazy<ConcurrentDictionary<string, IIndexValueType>>(() =>
            {
                var result = new ConcurrentDictionary<string, IIndexValueType>();

                foreach (var field in fieldDefinitionCollection)
                {
                    if (!string.IsNullOrWhiteSpace(field.Type) && ValueTypeFactories.TryGetFactory(field.Type, out var valueTypeFactory))
                    {
                        var valueType = valueTypeFactory.Create(field.Name);
                        valueType.SetupAnalyzers(Analyzer);
                        result.TryAdd(valueType.FieldName, valueType);
                    }
                    else
                    {
                        //Define the default!
                        if (!ValueTypeFactories.TryGetFactory(FieldDefinitionTypes.FullText, out var fullText))
                            throw new InvalidOperationException($"The value type factory {FieldDefinitionTypes.FullText} was not found");

                        var valueType = fullText.Create(field.Name);
                        valueType.SetupAnalyzers(Analyzer);
                        result.TryAdd(valueType.FieldName, valueType);
                    }
                }
                return result;
            });
        }

        /// <summary>
        /// Returns the <see cref="IIndexValueType"/> for the field name specified
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValueTypeFactory"></param>
        /// <returns></returns>
        /// <remarks>
        /// If it's not found it will create one with the factory supplied and initialize it.
        /// </remarks>
        public IIndexValueType GetValueType(string fieldName, IFieldValueTypeFactory fieldValueTypeFactory)
        {
            return _resolvedValueTypes.Value.GetOrAdd(fieldName, n =>
            {
                var t = fieldValueTypeFactory.Create(n);
                t.SetupAnalyzers(Analyzer);
                return t;
            });
        }

        /// <summary>
        /// Returns the value type for the field name specified
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns>Returns null if a value type was not found</returns>
        public IIndexValueType GetValueType(string fieldName)
        {
            return _resolvedValueTypes.Value.TryGetValue(fieldName, out var valueType) ? valueType : null;
        }


        /// <summary>
        /// Defines the field types such as number, fulltext, etc...
        /// </summary>
        /// <remarks>
        /// This collection is mutable but must be changed before the EnsureIndex method is fired (i.e. on startup)
        /// </remarks>
        public ValueTypeFactoryCollection ValueTypeFactories { get; } = new ValueTypeFactoryCollection();

        private readonly Lazy<ConcurrentDictionary<string, IIndexValueType>> _resolvedValueTypes;

        /// <summary>
        /// Returns the resolved collection of <see cref="IIndexValueType"/> for this index
        /// </summary>
        public IEnumerable<IIndexValueType> ValueTypes => _resolvedValueTypes.Value.Values;

    }
}