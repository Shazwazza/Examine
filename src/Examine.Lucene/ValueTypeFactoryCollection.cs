using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        /// <summary>
        /// Try get for the factory
        /// </summary>
        /// <param name="valueTypeName"></param>
        /// <param name="fieldValueTypeFactory"></param>
        /// <returns></returns>
        public bool TryGetFactory(string valueTypeName,
#if !NETSTANDARD2_0
            [MaybeNullWhen(false)]
#endif
            out IFieldValueTypeFactory fieldValueTypeFactory)
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

        /// <summary>
        /// The ammount of key/value pairs in the collection
        /// </summary>
        public int Count => _valueTypeFactories.Count;

        /// <summary>
        /// Returns the default index value types that is used in normal construction of an indexer
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyDictionary<string, IFieldValueTypeFactory> GetDefaultValueTypes(ILoggerFactory loggerFactory, Analyzer defaultAnalyzer)
            => GetDefaults(loggerFactory, defaultAnalyzer).ToDictionary(x => x.Key, x => (IFieldValueTypeFactory)new DelegateFieldValueTypeFactory(x.Value));

        private static IReadOnlyDictionary<string, Func<string, IIndexFieldValueType>> GetDefaults(ILoggerFactory loggerFactory, Analyzer? defaultAnalyzer = null) =>
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
                {FieldDefinitionTypes.FacetInteger, name => new Int32Type(name, true,false,loggerFactory, true)},
                {FieldDefinitionTypes.FacetFloat, name => new SingleType(name, true, false, loggerFactory, true)},
                {FieldDefinitionTypes.FacetDouble, name => new DoubleType(name,true, false, loggerFactory, true)},
                {FieldDefinitionTypes.FacetLong, name => new Int64Type(name, true, false, loggerFactory, true)},
                {FieldDefinitionTypes.FacetDateTime, name => new DateTimeType(name, true, true, false, loggerFactory, DateResolution.MILLISECOND)},
                {FieldDefinitionTypes.FacetDateYear, name => new DateTimeType(name,true, true, false, loggerFactory, DateResolution.YEAR)},
                {FieldDefinitionTypes.FacetDateMonth, name => new DateTimeType(name,true, true, false, loggerFactory, DateResolution.MONTH)},
                {FieldDefinitionTypes.FacetDateDay, name => new DateTimeType(name, true, true, false, loggerFactory, DateResolution.DAY)},
                {FieldDefinitionTypes.FacetDateHour, name => new DateTimeType(name,  true, true, false, loggerFactory, DateResolution.HOUR)},
                {FieldDefinitionTypes.FacetDateMinute, name => new DateTimeType(name, true, true, false, loggerFactory, DateResolution.MINUTE)},
                {FieldDefinitionTypes.FacetFullText, name => new FullTextType(name, loggerFactory, true, false, false, defaultAnalyzer ?? new CultureInvariantStandardAnalyzer())},
                {FieldDefinitionTypes.FacetFullTextSortable, name => new FullTextType(name, loggerFactory, true, false,true, defaultAnalyzer ?? new CultureInvariantStandardAnalyzer())},
                {FieldDefinitionTypes.FacetTaxonomyInteger, name => new Int32Type(name,true,true, loggerFactory, true)},
                {FieldDefinitionTypes.FacetTaxonomyFloat, name => new SingleType(name,isFacetable: true, taxonomyIndex: true, loggerFactory,  true)},
                {FieldDefinitionTypes.FacetTaxonomyDouble, name => new DoubleType(name, true, true, loggerFactory, true)},
                {FieldDefinitionTypes.FacetTaxonomyLong, name => new Int64Type(name, isFacetable: true, taxonomyIndex: true, loggerFactory, true)},
                {FieldDefinitionTypes.FacetTaxonomyDateTime, name => new DateTimeType(name,true, true, taxonomyIndex : true, loggerFactory, DateResolution.MILLISECOND)},
                {FieldDefinitionTypes.FacetTaxonomyDateYear, name =>  new DateTimeType(name, true, true, taxonomyIndex : true, loggerFactory, DateResolution.YEAR)},
                {FieldDefinitionTypes.FacetTaxonomyDateMonth, name => new DateTimeType(name, true, true, taxonomyIndex : true, loggerFactory, DateResolution.MONTH)},
                {FieldDefinitionTypes.FacetTaxonomyDateDay, name => new DateTimeType(name, true, true, taxonomyIndex : true, loggerFactory, DateResolution.DAY)},
                {FieldDefinitionTypes.FacetTaxonomyDateHour, name => new DateTimeType(name, true, isFacetable: true, taxonomyIndex: true, loggerFactory, DateResolution.HOUR)},
                {FieldDefinitionTypes.FacetTaxonomyDateMinute, name => new DateTimeType(name, true, true, taxonomyIndex : true, loggerFactory, DateResolution.MINUTE)},
                {FieldDefinitionTypes.FacetTaxonomyFullText, name => new FullTextType(name, loggerFactory, true, true, false, defaultAnalyzer ?? new CultureInvariantStandardAnalyzer())},
                {FieldDefinitionTypes.FacetTaxonomyFullTextSortable, name => new FullTextType(name, loggerFactory, true, true, true, defaultAnalyzer ?? new CultureInvariantStandardAnalyzer())},
                };


        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, IFieldValueTypeFactory>> GetEnumerator()
            => _valueTypeFactories.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
