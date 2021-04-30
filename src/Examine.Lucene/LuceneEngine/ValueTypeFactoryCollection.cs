using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Microsoft.Extensions.Logging;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// Manages the collection of <see cref="IFieldValueTypeFactory"/>
    /// </summary>
    public class ValueTypeFactoryCollection : IEnumerable<KeyValuePair<string, IFieldValueTypeFactory>>
    {
        public bool TryGetFactory(string valueTypeName, out IFieldValueTypeFactory fieldValueTypeFactory)
        {
            return _valueTypeFactories.TryGetValue(valueTypeName, out fieldValueTypeFactory);
        }

        public bool TryAdd(string valueTypeName, IFieldValueTypeFactory fieldValueTypeFactory)
        {
            return _valueTypeFactories.TryAdd(valueTypeName, fieldValueTypeFactory);
        }

        public bool TryAdd(string valueTypeName, Func<string, IIndexFieldValueType> fieldValueTypeFactory)
        {
            return _valueTypeFactories.TryAdd(valueTypeName, new DelegateFieldValueTypeFactory(fieldValueTypeFactory));
        }

        /// <summary>
        /// Replace any value type factory with the specified one, if one doesn't exist then it is added
        /// </summary>
        /// <param name="valueTypeName"></param>
        /// <param name="fieldValueTypeFactory"></param>
        public void AddOrUpdate(string valueTypeName, IFieldValueTypeFactory fieldValueTypeFactory)
        {
            _valueTypeFactories.AddOrUpdate(valueTypeName, fieldValueTypeFactory, (s, factory) => fieldValueTypeFactory);
        }

        /// <summary>
        /// Returns the <see cref="IFieldValueTypeFactory"/> by name, if it's not found an exception is thrown
        /// </summary>
        /// <param name="valueTypeName"></param>
        /// <returns></returns>
        public IFieldValueTypeFactory GetRequiredFactory(string valueTypeName)
        {
            if (!TryGetFactory(valueTypeName, out var fieldValueTypeFactory))
                throw new InvalidOperationException($"The required {typeof(IFieldValueTypeFactory).Name} was not found with name {valueTypeName}");

            return fieldValueTypeFactory;
        }

        public int Count => _valueTypeFactories.Count;

        private readonly ConcurrentDictionary<string, IFieldValueTypeFactory> _valueTypeFactories = new ConcurrentDictionary<string, IFieldValueTypeFactory>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Returns the default index value types that is used in normal construction of an indexer
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyDictionary<string, IFieldValueTypeFactory> GetDefaultValueTypes(ILoggerFactory loggerFactory, Analyzer defaultAnalyzer)
            => GetDefaults(loggerFactory, defaultAnalyzer).ToDictionary(x => x.Key, x => (IFieldValueTypeFactory)new DelegateFieldValueTypeFactory(x.Value));

        private static IReadOnlyDictionary<string, Func<string, IIndexFieldValueType>> GetDefaults(ILoggerFactory loggerFactory, Analyzer defaultAnalyzer = null) =>
            new Dictionary<string, Func<string, IIndexFieldValueType>>(StringComparer.InvariantCultureIgnoreCase) //case insensitive
            {
                {"number", name => new Int32Type(name, loggerFactory.CreateLogger<Int32Type>())},
                {FieldDefinitionTypes.Integer, name => new Int32Type(name, loggerFactory.CreateLogger<Int32Type>())},
                {FieldDefinitionTypes.Float, name => new SingleType(name, loggerFactory.CreateLogger<SingleType>())},
                {FieldDefinitionTypes.Double, name => new DoubleType(name, loggerFactory.CreateLogger<DoubleType>())},
                {FieldDefinitionTypes.Long, name => new Int64Type(name, loggerFactory.CreateLogger<Int64Type>())},
                {"date", name => new DateTimeType(name, loggerFactory.CreateLogger<DateTimeType>(), DateTools.Resolution.MILLISECOND)},
                {FieldDefinitionTypes.DateTime, name => new DateTimeType(name, loggerFactory.CreateLogger<DateTimeType>(), DateTools.Resolution.MILLISECOND)},
                {FieldDefinitionTypes.DateYear, name => new DateTimeType(name, loggerFactory.CreateLogger<DateTimeType>(), DateTools.Resolution.YEAR)},
                {FieldDefinitionTypes.DateMonth, name => new DateTimeType(name, loggerFactory.CreateLogger<DateTimeType>(), DateTools.Resolution.MONTH)},
                {FieldDefinitionTypes.DateDay, name => new DateTimeType(name, loggerFactory.CreateLogger<DateTimeType>(), DateTools.Resolution.DAY)},
                {FieldDefinitionTypes.DateHour, name => new DateTimeType(name, loggerFactory.CreateLogger<DateTimeType>(), DateTools.Resolution.HOUR)},
                {FieldDefinitionTypes.DateMinute, name => new DateTimeType(name, loggerFactory.CreateLogger<DateTimeType>(), DateTools.Resolution.MINUTE)},
                {FieldDefinitionTypes.Raw, name => new RawStringType(name, loggerFactory.CreateLogger<RawStringType>())},
                // TODO: This is the default and it doesn't use the default analyzer
                {FieldDefinitionTypes.FullText, name => new FullTextType(name, loggerFactory.CreateLogger<FullTextType>(), defaultAnalyzer)},
                {FieldDefinitionTypes.FullTextSortable, name => new FullTextType(name, loggerFactory.CreateLogger<FullTextType>(), defaultAnalyzer, true)},
                {FieldDefinitionTypes.InvariantCultureIgnoreCase, name => new GenericAnalyzerFieldValueType(name, loggerFactory.CreateLogger<GenericAnalyzerFieldValueType>(), new CultureInvariantWhitespaceAnalyzer())},
                {FieldDefinitionTypes.EmailAddress, name => new GenericAnalyzerFieldValueType(name, loggerFactory.CreateLogger<GenericAnalyzerFieldValueType>(), new EmailAddressAnalyzer())}
            };


        public IEnumerator<KeyValuePair<string, IFieldValueTypeFactory>> GetEnumerator()
        {
            return _valueTypeFactories.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
