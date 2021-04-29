using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Examine.LuceneEngine
{
    
    internal class OpenReaderTracker
    {
        private static readonly OpenReaderTracker Instance = new OpenReaderTracker();

        private readonly Dictionary<Directory, Queue<Tuple<IndexReader, DateTime>>> _oldReaders = new Dictionary<Directory, Queue<Tuple<IndexReader, DateTime>>>();

        private readonly object _locker = new object();

        public static OpenReaderTracker Current
        {
            get { return Instance; }
        }

        public void AddOpenReader(IndexReader reader, Directory directory)
        {
            lock (_locker)
            {
                var dir = reader.Directory();
                if (!_oldReaders.TryGetValue(dir, out var readers))
                {
                    readers = new Queue<Tuple<IndexReader, DateTime>>();
                    _oldReaders.Add(dir, readers);
                }

                readers.Enqueue(new Tuple<IndexReader, DateTime>(reader, DateTime.Now));
            }
        }

        public int CloseStaleReaders(Directory dir, TimeSpan ts)
        {
            lock (_locker)
            {
                var now = DateTime.Now;

                if (!_oldReaders.TryGetValue(dir, out var readersForDir))
                {
                    return 0;
                }

                var hasStale = true;
                var staleCount = 0;

                while (hasStale)
                {
                    var oldest = readersForDir.Peek();
                    hasStale = now - oldest.Item2 >= ts;
                    if (hasStale)
                    {
                        staleCount++;
                        //close reader and remove from list
                        try
                        {
                            oldest.Item1.Dispose();
                        }
                        catch (Exception e) when (e is AlreadyClosedException || e is ObjectDisposedException)
                        {
                            //if this happens, more than one instance has decreased referenced, this could occur if this 
                            //somehow gets called in conjuction with the shutdown code or manually, etc...
                        }
                        finally
                        {
                            readersForDir.Dequeue();
                        }
                    }
                }

                return staleCount;
            }
            
        }

        public int CloseAllReaders(Directory dir)
        {
            lock (_locker)
            {
                if (!_oldReaders.TryGetValue(dir, out var readersForDir))
                {
                    return 0;
                }

                var readerCount = readersForDir.Count;

                while (readersForDir.Count > 0)
                {
                    var reader = readersForDir.Dequeue();

                    //close reader
                    try
                    {
                        reader.Item1.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                        //if this happens, more than one instance has decreased referenced, this could occur if this 
                        //somehow gets called in conjuction with the shutdown code or manually, etc...
                    }
                }

                return readerCount;
            }
        }

        public int CloseAllReaders()
        {
            lock (_locker)
            {
                var readerCount = 0;

                foreach (var kv in _oldReaders)
                {
                    readerCount += kv.Value.Count;

                    while (kv.Value.Count > 0)
                    {
                        var reader = kv.Value.Dequeue();

                        //close reader
                        try
                        {
                            reader.Item1.Dispose();
                        }
                        catch (Exception e) when (e is AlreadyClosedException || e is ObjectDisposedException)
                        {
                            //if this happens, more than one instance has decreased referenced, this could occur if this 
                            //somehow gets called in conjuction with the shutdown code or manually, etc...
                        }
                    }
                }

                _oldReaders.Clear();

                return readerCount;
            }
        }

    }
}
