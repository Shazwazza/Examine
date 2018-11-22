using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Documents;
using Lucene.Net.Store;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// Tracks the value types for fields for each index by a directory, used only for config based indexes/searchers
    /// </summary>
    internal class FieldValueTypesTracker
    {
        private readonly ConcurrentDictionary<string, Lazy<FieldValueTypeCollection>> _fieldValueTypes = new ConcurrentDictionary<string, Lazy<FieldValueTypeCollection>>();

        public static FieldValueTypesTracker Current { get; } = new FieldValueTypesTracker();

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
            if (!_fieldValueTypes.TryGetValue(dir.GetLockId(), out var d))
            {
                if (throwIfEmpty)
                {
                    throw new NullReferenceException("No index field types were added with directory key " + dir.GetLockId() + ", maybe an indexer hasn't been initialized?");
                }
            }
            return d.Value;
        }

        public Lazy<FieldValueTypeCollection> InitializeFieldValueTypes(Directory dir, Func<Directory, Lazy<FieldValueTypeCollection>> factory)
        {
            var resolved = _fieldValueTypes.GetOrAdd(dir.GetLockId(), s => factory(dir));
            return resolved;
        }

        
    }
}