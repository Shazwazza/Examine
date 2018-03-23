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
        
        private readonly ConcurrentDictionary<string, IndexFieldValueTypes> _fieldValueTypes = new ConcurrentDictionary<string, IndexFieldValueTypes>();

        public static FieldValueTypes Current { get; } = new FieldValueTypes();

        public IndexFieldValueTypes GetIndexFieldValueTypes(Directory dir)
        {
            return GetIndexFieldValueTypes(dir, false);
        }

        public IndexFieldValueTypes GetIndexFieldValueTypes(Directory dir, bool throwIfEmpty)
        {
            IndexFieldValueTypes d = null;
            if (!_fieldValueTypes.TryGetValue(dir.GetLockId(), out d))
            {
                if (throwIfEmpty)
                {
                    throw new NullReferenceException("No index field types were added with directory key " + dir.GetLockId() + ", maybe an indexer hasn't been initialized?");
                }
            }
            return d;
        }

        public IndexFieldValueTypes GetIndexFieldValueTypes(Directory dir, Func<Directory, IndexFieldValueTypes> factory)
        {
            var resolved = _fieldValueTypes.GetOrAdd(dir.GetLockId(), s => factory(dir));
            return resolved;
        }

        /// <summary>
        /// Returns the default index value types that is used in normal construction of an indexer
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, Func<string, IIndexValueType>> DefaultIndexValueTypes
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
                {FieldDefinitionTypes.FullTextSortable, name => new FullTextType(name, true)}
            };
    }
}