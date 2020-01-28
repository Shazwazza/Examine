using System;
using System.Collections.Concurrent;
using System.Security;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Examine.LuceneEngine
{
    
    public sealed class WriterTracker
    {
        /// <summary>
        /// Used for tests
        /// </summary>
        internal void Reset()
        {
            _writers.Clear();
        }

        private readonly ConcurrentDictionary<string, IndexWriter> _writers = new ConcurrentDictionary<string, IndexWriter>();

        public static WriterTracker Current { get; } = new WriterTracker();

        public IndexWriter GetWriter(Directory dir)
        {
            return GetWriter(dir, false);
        }

        public IndexWriter GetWriter(Directory dir, bool throwIfEmpty)
        {
            IndexWriter d = null;
            if (!_writers.TryGetValue(dir.GetLockId(), out d))
            {
                if (throwIfEmpty)
                {
                    throw new NullReferenceException("No writer was added with directory key " + dir.GetLockId() + ", maybe an indexer hasn't been initialized?");
                }
            }
            return d;
        }

        public IndexWriter GetWriter(Directory dir, Func<Directory, IndexWriter> factory)
        {
            var resolved = _writers.GetOrAdd(dir.GetLockId(), s => factory(dir));
            return resolved;
        }

    }
}