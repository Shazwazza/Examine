using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Documents;
using Lucene.Net.Store;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// Tracks the value types for fields for each index by a directory
    /// </summary>
    public class FieldValueTypes
    {
        /// <summary>
        /// Used for tests
        /// </summary>
        internal void Reset()
        {
            _fieldValueTypes.Clear();
        }
        
        private readonly ConcurrentDictionary<string, FieldValueTypeCollection> _fieldValueTypes = new ConcurrentDictionary<string, FieldValueTypeCollection>();

        public static FieldValueTypes Current { get; } = new FieldValueTypes();

        /// <summary>
        /// Returns the <see cref="FieldValueTypeCollection"/> for the directory/index
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public FieldValueTypeCollection GetIndexFieldValueTypes(Directory dir)
        {
            return GetIndexFieldValueTypes(dir, true);
        }

        /// <summary>
        /// Returns the <see cref="FieldValueTypeCollection"/> for the directory/index
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="throwIfEmpty"></param>
        /// <returns></returns>
        public FieldValueTypeCollection GetIndexFieldValueTypes(Directory dir, bool throwIfEmpty)
        {
            FieldValueTypeCollection d;
            if (!_fieldValueTypes.TryGetValue(dir.GetLockId(), out d))
            {
                if (throwIfEmpty)
                {
                    throw new NullReferenceException("No index field types were added with directory key " + dir.GetLockId() + ", maybe an indexer hasn't been initialized?");
                }
            }
            return d;
        }

        public FieldValueTypeCollection InitializeFieldValueTypes(Directory dir, Func<Directory, FieldValueTypeCollection> factory)
        {
            var resolved = _fieldValueTypes.GetOrAdd(dir.GetLockId(), s => factory(dir));
            return resolved;
        }

        /// <summary>
        /// Returns the default index value types that is used in normal construction of an indexer
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyDictionary<string, Func<string, IIndexValueType>> DefaultIndexValueTypes
            => new Dictionary<string, Func<string, IIndexValueType>>(StringComparer.InvariantCultureIgnoreCase) //case insensitive
            {
                {"number", name => new Int32Type(name)},
                {FieldDefinitionTypes.Integer, name => new Int32Type(name)},
                {FieldDefinitionTypes.Float, name => new SingleType(name)},
                {FieldDefinitionTypes.Double, name => new DoubleType(name)},
                {FieldDefinitionTypes.Long, name => new Int64Type(name)},
                {"date", name => new DateTimeType(name, DateTools.Resolution.MILLISECOND)},
                {FieldDefinitionTypes.DateTime, name => new DateTimeType(name, DateTools.Resolution.MILLISECOND)},
                {FieldDefinitionTypes.DateYear, name => new DateTimeType(name, DateTools.Resolution.YEAR)},
                {FieldDefinitionTypes.DateMonth, name => new DateTimeType(name, DateTools.Resolution.MONTH)},
                {FieldDefinitionTypes.DateDay, name => new DateTimeType(name, DateTools.Resolution.DAY)},
                {FieldDefinitionTypes.DateHour, name => new DateTimeType(name, DateTools.Resolution.HOUR)},
                {FieldDefinitionTypes.DateMinute, name => new DateTimeType(name, DateTools.Resolution.MINUTE)},
                {FieldDefinitionTypes.Raw, name => new RawStringType(name)},
                {FieldDefinitionTypes.FullText, name => new FullTextType(name)},
                {FieldDefinitionTypes.FullTextSortable, name => new FullTextType(name, true)},
                {FieldDefinitionTypes.InvariantCultureIgnoreCase, name => new GenericAnalyzerValueType(name, new CultureInvariantWhitespaceAnalyzer())}
            };
    }
}