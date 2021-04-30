using System.Collections.Concurrent;
using System.Threading;

namespace Examine.Lucene.Directories
{
    /// <summary>
    /// A class to manage mutex locks per directory instance (ID)
    /// </summary>
    public sealed class SyncMutexManager
    {
        private static readonly ConcurrentDictionary<global::Lucene.Net.Store.Directory, SyncMutexManager> s_mutexManagers
            = new ConcurrentDictionary<global::Lucene.Net.Store.Directory, SyncMutexManager>();

        private Mutex CreateMutex() => new Mutex(false);

        public Mutex GrabMutex(global::Lucene.Net.Store.Directory directory)
        {
            var mgr = s_mutexManagers.GetOrAdd(directory, d => new SyncMutexManager());
            return mgr.CreateMutex();
        }
    }
}
