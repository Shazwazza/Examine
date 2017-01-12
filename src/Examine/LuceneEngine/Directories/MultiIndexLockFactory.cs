using System;
using System.Security;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Directories
{
    /// <summary>
    /// Lock factory that wraps multiple factories
    /// </summary>
    [SecurityCritical]
    public class MultiIndexLockFactory : LockFactory
    {
        private readonly LockFactory _master;
        private readonly LockFactory _child;
        
        public MultiIndexLockFactory(Directory master, Directory child)
        {
            if (master == null) throw new ArgumentNullException("master");
            if (child == null) throw new ArgumentNullException("child");
            _master = master.GetLockFactory();
            _child = child.GetLockFactory();
        }
        
        public MultiIndexLockFactory(LockFactory master, LockFactory child)
        {
            if (master == null) throw new ArgumentNullException("master");
            if (child == null) throw new ArgumentNullException("child");
            _master = master;
            _child = child;
        }

        [SecurityCritical]
        public override Lock MakeLock(string lockName)
        {
            return new MultiIndexLock(_master.MakeLock(lockName), _child.MakeLock(lockName));
        }

        [SecurityCritical]
        public override void ClearLock(string lockName)
        {
            _master.ClearLock(lockName);
            _child.ClearLock(lockName);
        }
    }
}