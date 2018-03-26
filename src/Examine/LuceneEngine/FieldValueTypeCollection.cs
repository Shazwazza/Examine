using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Analysis;

namespace Examine.LuceneEngine
{
    public class FieldValueTypeCollection
    {
        /// <summary>
        /// Returns the PerFieldAnalyzerWrapper
        /// </summary>
        internal PerFieldAnalyzerWrapper Analyzer { get; }

        public FieldValueTypeCollection(
            Analyzer defaultAnalyzer,
            IEnumerable<KeyValuePair<string, Func<string, IIndexValueType>>> types, FieldDefinitionCollection fieldDefinitionCollection)
        {
            Analyzer = new PerFieldAnalyzerWrapper(defaultAnalyzer);
            foreach (var type in types)
            {
                ValueTypeFactories.TryAdd(type.Key, type.Value);
            }

            //initializes the collection of field aliases to it's correct IIndexValueType
            _resolvedValueTypes = new Lazy<ConcurrentDictionary<string, IIndexValueType>>(() =>
            {
                var result = new ConcurrentDictionary<string, IIndexValueType>();

                foreach (var field in fieldDefinitionCollection)
                {
                    if (!string.IsNullOrWhiteSpace(field.Value.Type) && ValueTypeFactories.TryGetValue(field.Value.Type, out var valueTypeFactory))
                    {
                        var valueType = valueTypeFactory(field.Key);
                        valueType.SetupAnalyzers(Analyzer);
                        result.TryAdd(valueType.FieldName, valueType);
                    }
                    else
                    {
                        //Define the default!
                        var fulltext = ValueTypeFactories[FieldDefinitionTypes.FullText];
                        var valueType = fulltext(field.Key);
                        valueType.SetupAnalyzers(Analyzer);
                        result.TryAdd(valueType.FieldName, valueType);
                    }
                }
                return result;
            });
        }

        /// <summary>
        /// Returns the value type for the field name specified, if it's not found it will create on with the factory supplied
        /// and initialize it.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="indexValueTypeFactory"></param>
        /// <returns></returns>
        public IIndexValueType GetValueType(string fieldName, Func<string, IIndexValueType> indexValueTypeFactory)
        {
            return _resolvedValueTypes.Value.GetOrAdd(fieldName, n =>
            {
                var t = indexValueTypeFactory(n);
                t.SetupAnalyzers(Analyzer);
                return t;
            });
        }

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
        public ConcurrentDictionary<string, Func<string, IIndexValueType>> ValueTypeFactories { get; } = new ConcurrentDictionary<string, Func<string, IIndexValueType>>(StringComparer.InvariantCultureIgnoreCase);

        private readonly Lazy<ConcurrentDictionary<string, IIndexValueType>> _resolvedValueTypes;

        /// <summary>
        /// Returns the resolved collection of IIndexValueTypes for this index
        /// </summary>
        public IEnumerable<IIndexValueType> ValueTypes => _resolvedValueTypes.Value.Values;
    }
}