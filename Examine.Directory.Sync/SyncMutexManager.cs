using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Examine.Directory.Sync
{
    internal static class SyncMutexManager
    {
        public static Mutex GrabMutex(string name)
        {
            var mutexName = "examineSyncSegmentMutex_" + name;

            Mutex mutex;
            var notExisting = false;
            
            if (Mutex.TryOpenExisting(mutexName, MutexRights.Synchronize | MutexRights.Modify, out mutex))
            {
                return mutex;
            }

            // Here we know the mutex either doesn't exist or we don't have the necessary permissions.

            if (!Mutex.TryOpenExisting(mutexName, MutexRights.ReadPermissions | MutexRights.ChangePermissions, out mutex))
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
                return new Mutex(false, mutexName, out mutexIsNew, security);
            }
            else
            {
                var m = Mutex.OpenExisting(mutexName, MutexRights.ReadPermissions | MutexRights.ChangePermissions);
                var security = m.GetAccessControl();
                var user = Environment.UserDomainName + "\\" + Environment.UserName;
                var rule = new MutexAccessRule(user, MutexRights.Synchronize | MutexRights.Modify, AccessControlType.Allow);
                security.AddAccessRule(rule);
                m.SetAccessControl(security);

                return Mutex.OpenExisting(mutexName);
            }
        }
    }
}
