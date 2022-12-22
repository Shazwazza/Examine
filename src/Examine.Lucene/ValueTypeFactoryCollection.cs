using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Examine.Lucene.Analyzers;
using Examine.Lucene.Indexing;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene
{
    /// <summary>
    /// Manages the collection of <see cref="IFieldValueTypeFactory"/>
    /// </summary>
    public class ValueTypeFactoryCollection : IEnumerable<KeyValuePair<string, IFieldValueTypeFactory>>
    {
        private readonly ConcurrentDictionary<string, IFieldValueTypeFactory> _valueTypeFactories;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="valueTypeFactories"></param>
        public ValueTypeFactoryCollection(IReadOnlyDictionary<string, IFieldValueTypeFactory> valueTypeFactories)
            => _valueTypeFactories = new ConcurrentDictionary<string, IFieldValueTypeFactory>(
                valueTypeFactories,
                StringComparer.InvariantCultureIgnoreCase);

        public bool TryGetFactory(string valueTypeName, out IFieldValueTypeFactory fieldValueTypeFactory)
            => _valueTypeFactories.TryGetValue(valueTypeName, out fieldValueTypeFactory);

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

        /// <summary>
        /// Returns the default index value types that is used in normal construction of an indexer
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyDictionary<string, IFieldValueTypeFactory> GetDefaultValueTypes(ILoggerFactory loggerFactory, Analyzer defaultAnalyzer)
            => GetDefaults(loggerFactory, defaultAnalyzer).ToDictionary(x => x.Key, x => (IFieldValueTypeFactory)new DelegateFieldValueTypeFactory(x.Value));

        private static IReadOnlyDictionary<string, Func<string, IIndexFieldValueType>> GetDefaults(ILoggerFactory loggerFactory, Analyzer defaultAnalyzer = null) =>
            new Dictionary<string, Func<string, IIndexFieldValueType>>(StringComparer.InvariantCultureIgnoreCase) //case insensitive
            {
                {"number", name => new Int32Type(name, loggerFactory)},
                {FieldDefinitionTypes.Integer, name => new Int32Type(name, loggerFactory)},
                {FieldDefinitionTypes.Float, name => new SingleType(name, loggerFactory)},
                {FieldDefinitionTypes.Double, name => new DoubleType(name, loggerFactory)},
                {FieldDefinitionTypes.Long, name => new Int64Type(name, loggerFactory)},
                {"date", name => new DateTimeType(name, loggerFactory, DateResolution.MILLISECOND)},
                {FieldDefinitionTypes.DateTime, name => new DateTimeType(name, loggerFactory, DateResolution.MILLISECOND)},
                {FieldDefinitionTypes.DateYear, name => new DateTimeType(name, loggerFactory, DateResolution.YEAR)},
                {FieldDefinitionTypes.DateMonth, name => new DateTimeType(name, loggerFactory, DateResolution.MONTH)},
                {FieldDefinitionTypes.DateDay, name => new DateTimeType(name, loggerFactory, DateResolution.DAY)},
                {FieldDefinitionTypes.DateHour, name => new DateTimeType(name, loggerFactory, DateResolution.HOUR)},
                {FieldDefinitionTypes.DateMinute, name => new DateTimeType(name, loggerFactory, DateResolution.MINUTE)},
                {FieldDefinitionTypes.Raw, name => new RawStringType(name, loggerFactory)},
                {FieldDefinitionTypes.FullText, name => new FullTextType(name, loggerFactory, defaultAnalyzer)},
                {FieldDefinitionTypes.FullTextSortable, name => new FullTextType(name, loggerFactory, defaultAnalyzer, true)},
                {FieldDefinitionTypes.InvariantCultureIgnoreCase, name => new GenericAnalyzerFieldValueType(name, loggerFactory, new CultureInvariantWhitespaceAnalyzer())},
                {FieldDefinitionTypes.EmailAddress, name => new GenericAnalyzerFieldValueType(name, loggerFactory, new EmailAddressAnalyzer())},
                {FieldDefinitionTypes.FacetInteger, name => new FacetInt32Type(name, loggerFactory)},
                {FieldDefinitionTypes.FacetFloat, name => new FacetSingleType(name, loggerFactory)},
                {FieldDefinitionTypes.FacetDouble, name => new FacetDoubleType(name, loggerFactory)},
                {FieldDefinitionTypes.FacetLong, name => new FacetInt64Type(name, loggerFactory)},
                {FieldDefinitionTypes.FacetDateTime, name => new FacetDateTimeType(name, loggerFactory, DateResolution.MILLISECOND)},
                {FieldDefinitionTypes.FacetDateYear, name => new FacetDateTimeType(name, loggerFactory, DateResolution.YEAR)},
                {FieldDefinitionTypes.FacetDateMonth, name => new FacetDateTimeType(name, loggerFactory, DateResolution.MONTH)},
                {FieldDefinitionTypes.FacetDateDay, name => new FacetDateTimeType(name, loggerFactory, DateResolution.DAY)},
                {FieldDefinitionTypes.FacetDateHour, name => new FacetDateTimeType(name, loggerFactory, DateResolution.HOUR)},
                {FieldDefinitionTypes.FacetDateMinute, name => new FacetDateTimeType(name, loggerFactory, DateResolution.MINUTE)},
                {FieldDefinitionTypes.FacetFullText, name => new FacetFullTextType(name, loggerFactory, defaultAnalyzer)},
                {FieldDefinitionTypes.FacetFullTextSortable, name => new FacetFullTextType(name, loggerFactory, defaultAnalyzer, true)},
                {FieldDefinitionTypes.FacetTaxonomyInteger, name => new FacetInt32Type(name, loggerFactory)},
                {FieldDefinitionTypes.FacetTaxonomyFloat, name => new FacetSingleType(name, loggerFactory)},
                {FieldDefinitionTypes.FacetTaxonomyDouble, name => new FacetDoubleType(name, loggerFactory)},
                {FieldDefinitionTypes.FacetTaxonomyLong, name => new FacetInt64Type(name, loggerFactory)},
                {FieldDefinitionTypes.FacetTaxonomyDateTime, name => new FacetDateTimeType(name, loggerFactory, DateResolution.MILLISECOND, taxonomyIndex: true)},
                {FieldDefinitionTypes.FacetTaxonomyDateYear, name => new FacetDateTimeType(name, loggerFactory, DateResolution.YEAR, taxonomyIndex: true)},
                {FieldDefinitionTypes.FacetTaxonomyDateMonth, name => new FacetDateTimeType(name, loggerFactory, DateResolution.MONTH, taxonomyIndex: true)},
                {FieldDefinitionTypes.FacetTaxonomyDateDay, name => new FacetDateTimeType(name, loggerFactory, DateResolution.DAY, taxonomyIndex: true)},
                {FieldDefinitionTypes.FacetTaxonomyDateHour, name => new FacetDateTimeType(name, loggerFactory, DateResolution.HOUR, taxonomyIndex: true)},
                {FieldDefinitionTypes.FacetTaxonomyDateMinute, name => new FacetDateTimeType(name, loggerFactory, DateResolution.MINUTE, taxonomyIndex: true)},
                {FieldDefinitionTypes.FacetTaxonomyFullText, name => new FacetFullTextType(name, loggerFactory, defaultAnalyzer, false, taxonomyIndex: true)},
                {FieldDefinitionTypes.FacetTaxonomyFullTextSortable, name => new FacetFullTextType(name, loggerFactory, defaultAnalyzer, true, taxonomyIndex: true)},
                };


        public IEnumerator<KeyValuePair<string, IFieldValueTypeFactory>> GetEnumerator()
            => _valueTypeFactories.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
