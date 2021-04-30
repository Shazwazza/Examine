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
    
    public class SyncMutexManager
    {
        private readonly string _id;

        
        private static readonly ConcurrentDictionary<Lucene.Net.Store.Directory, SyncMutexManager> MutexManagers = new ConcurrentDictionary<Lucene.Net.Store.Directory, SyncMutexManager>();

        
        public SyncMutexManager(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            _id = id;
        }

        private Mutex CreateMutex(string name)
        {
            var mutexName = string.Concat("examineSegmentMutex_", _id, "_", name);

            Mutex mutex;
            var notExisting = false;
            //todo: find how to use MutexRights.ReadPermissions | MutexRights.ChangePermissions
            if (Mutex.TryOpenExisting(mutexName , out mutex))
            {
                return mutex;
            }

            // Here we know the mutex either doesn't exist or we don't have the necessary permissions.

            if (!Mutex.TryOpenExisting(mutexName, out mutex))
            {
                notExisting = true;
            }

         
            if (notExisting)
            {
                var worldSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                var security = new MutexSecurity();
                var rule = new MutexAccessRule(worldSid, MutexRights.FullControl, AccessControlType.Allow);
         

                security.AddAccessRule(rule);
                var mutexIsNew = false;
                //todo: find how to use security
                return new Mutex(false, mutexName, out mutexIsNew);
            }
            else
            {
                //todo: find how to use MutexRights.ReadPermissions | MutexRights.ChangePermissions
                var m = Mutex.OpenExisting(mutexName);
                var security = m.GetAccessControl();
                var user = Environment.UserDomainName + "\\" + Environment.UserName;
                var rule = new MutexAccessRule(user, MutexRights.Synchronize | MutexRights.Modify, AccessControlType.Allow);
                security.AddAccessRule(rule);
                m.SetAccessControl(security);

                return Mutex.OpenExisting(mutexName);
            }
        }

        public static Mutex GrabMutex(Lucene.Net.Store.Directory directory, string name)
        {
            var mgr = MutexManagers.GetOrAdd(directory, d => new SyncMutexManager(d.GetLockID()));
            return mgr.CreateMutex(name);
        }
    }
}
