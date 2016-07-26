using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// Used to retrieve/track the same lucene directory instance for a given DirectoryInfo object
    /// </summary>
    [SecuritySafeCritical]
    public sealed class DirectoryTracker
    {
        private static readonly DirectoryTracker Instance = new DirectoryTracker();
       
        private readonly ConcurrentDictionary<string, Directory> _directories = new ConcurrentDictionary<string, Directory>();
   
        public static DirectoryTracker Current
        {
            get { return Instance; }
        }

        public Directory GetDirectory(DirectoryInfo dir)
        {
            return GetDirectory(dir, false);
        }

        public Directory GetDirectory(DirectoryInfo dir, bool throwIfEmpty)
        {
            if (throwIfEmpty)
            {
                Directory d;
                if (!_directories.TryGetValue(dir.FullName, out d))
                {
                    throw new NullReferenceException("No directory was added with path " + dir.FullName + ", maybe an indexer hasn't been initialized?");
                }
                return d;
            }
            var resolved = _directories.GetOrAdd(dir.FullName, s => new SimpleFSDirectory(dir));
            return resolved;
        }

        public Directory GetDirectory(DirectoryInfo dir, Func<string, Directory> factory)
        {
            var resolved = _directories.GetOrAdd(dir.FullName, factory);
            return resolved;
        }
    }
}
