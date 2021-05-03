using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly Dictionary<Directory, Queue<Tuple<IndexWriter, DateTime>>> _oldWriters = new Dictionary<Directory, Queue<Tuple<IndexWriter, DateTime>>>();

        private readonly object _locker = new object();
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
            lock (_locker)
            {
                var resolved = _writers.GetOrAdd(dir.GetLockId(), s => factory(dir));
                return resolved;
            }
        }
        ///<summary>
        /// That code is tagging writer as not in use anymore, so we can safely dispose them when they are not in use anymore.
        ///</summary>
        public void MapAsOld(Directory dir)
        {
            lock (_locker)
            {
                if (!_writers.TryGetValue(dir.GetLockId(), out IndexWriter d))
                {
                  var  readers = new Queue<Tuple<IndexWriter, DateTime>>();
                  readers.Enqueue(new Tuple<IndexWriter, DateTime>(d, DateTime.Now));
                  _oldWriters.Add(dir, readers);
                }

                _writers.TryRemove(dir.GetLockId(), out d);

                return;
            }

        }
        public int CloseStaleWriters(Directory dir, TimeSpan ts)
        {
            lock (_locker)
            {
                var now = DateTime.Now;

                if (!_oldWriters.TryGetValue(dir, out var writerForDir))
                {
                    return 0;
                }

                var hasStale = true;
                var staleCount = 0;

                while (hasStale)
                {
                    var oldest = writerForDir.Peek();
                    hasStale = now - oldest.Item2 >= ts;
                    if (hasStale)
                    {
                        staleCount++;
                        //close writer and remove from list
                        try
                        {
                            oldest.Item1.Dispose();
                        }
                        catch (AlreadyClosedException)
                        {
                            //if this happens, more than one instance has decreased referenced, this could occur if this 
                            //somehow gets called in conjuction with the shutdown code or manually, etc...
                        }
                        finally
                        {
                            writerForDir.Dequeue();
                        }
                    }
                }

                return staleCount;
            }
            
        }
    }
}