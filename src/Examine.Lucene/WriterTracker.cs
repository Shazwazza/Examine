using System;
using System.Collections.Concurrent;
using System.Security;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Examine.Lucene
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
            if (!_writers.TryGetValue(dir.GetLockID(), out d))
            {
                if (throwIfEmpty)
                {
                    throw new NullReferenceException("No writer was added with directory key " + dir.GetLockID() + ", maybe an indexer hasn't been initialized?");
                }
            }
            return d;
        }

        public IndexWriter GetWriter(Directory dir, Func<Directory, IndexWriter> factory)
        {
            var resolved = _writers.GetOrAdd(dir.GetLockID(), s => factory(dir));
            return resolved;
        }

    }
}