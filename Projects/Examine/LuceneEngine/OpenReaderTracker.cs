using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Examine.LuceneEngine
{
    [SecuritySafeCritical]
    internal class OpenReaderTracker
    {
        private static readonly OpenReaderTracker Instance = new OpenReaderTracker();

        private readonly List<Tuple<IndexReader, DateTime>> _oldReaders = new List<Tuple<IndexReader, DateTime>>(); 

        private readonly object _locker = new object();

        public static OpenReaderTracker Current
        {
            get { return Instance; }
        }

        public void AddOpenReader(IndexReader reader)
        {
            lock (_locker)
            {
                reader.IncRef();
                _oldReaders.Add(new Tuple<IndexReader, DateTime>(reader, DateTime.Now));
            }
        }

        public int CloseStaleReaders(TimeSpan ts)
        {
            lock (_locker)
            {
                var now = DateTime.Now;

                var currReaders = _oldReaders.ToList();
                var newest = currReaders.OrderByDescending(x => x.Item2).FirstOrDefault();
                currReaders.Remove(newest);
                var stale = currReaders.Where(x => now - x.Item2 >= ts).ToArray();

                foreach (var reader in stale)
                {
                    //close reader and remove from list
                    reader.Item1.DecRef();
                    _oldReaders.Remove(reader);
                }
                return stale.Length;
            }
            
        }

        public int CloseAllReaders(Directory dir)
        {
            lock (_locker)
            {
                var readers = _oldReaders.Where(x => x.Item1.Directory().GetLockID() == dir.GetLockID()).ToArray();
                foreach (var reader in readers)
                {
                    //close reader and remove from list
                    //reader.Item1.DecRef();
                    reader.Item1.Close();
                    _oldReaders.Remove(reader);
                }
                return readers.Length;
            }
        }

        public int CloseAllReaders()
        {
            lock (_locker)
            {
                var readers = _oldReaders.ToArray();
                foreach (var reader in readers)
                {
                    //close reader and remove from list
                    //reader.Item1.DecRef();
                    reader.Item1.Close();
                    _oldReaders.Remove(reader);
                }
                return readers.Length;
            }
        }

    }
}