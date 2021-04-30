using System;
using System.Collections.Concurrent;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace Examine.LuceneEngine.Directories
{
    /// <summary>
    /// A class to manage mutex locks per directory instance (ID)
    /// </summary>
    public sealed class SyncMutexManager
    {
        private static readonly ConcurrentDictionary<Lucene.Net.Store.Directory, SyncMutexManager> s_mutexManagers
            = new ConcurrentDictionary<Lucene.Net.Store.Directory, SyncMutexManager>();

        private Mutex CreateMutex() => new Mutex(false);

        public Mutex GrabMutex(Lucene.Net.Store.Directory directory)
        {
            var mgr = s_mutexManagers.GetOrAdd(directory, d => new SyncMutexManager());
            return mgr.CreateMutex();
        }
    }
}
