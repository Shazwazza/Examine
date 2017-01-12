using System;
using Lucene.Net.Store;

namespace Examine.Directory.Sync
{
    /// <summary>
    /// Lock factory that wraps multiple factories
    /// </summary>
    public class MultiIndexLockFactory : LockFactory
    {
        private readonly LockFactory _master;
        private readonly LockFactory _child;

        public MultiIndexLockFactory(Lucene.Net.Store.Directory master, Lucene.Net.Store.Directory child)
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

        public override Lock MakeLock(string lockName)
        {
            return new MultiIndexLock(_master.MakeLock(lockName), _child.MakeLock(lockName));
        }

        public override void ClearLock(string lockName)
        {
            _master.ClearLock(lockName);
            _child.ClearLock(lockName);
        }
    }
}