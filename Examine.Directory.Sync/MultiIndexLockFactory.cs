using System;
using Lucene.Net.Store;

namespace Examine.Directory.Sync
{
    /// <summary>
    /// Lock factory that wraps multiple factories
    /// </summary>
    internal class MultiIndexLockFactory : LockFactory
    {
        private readonly Lucene.Net.Store.Directory _master;
        private readonly Lucene.Net.Store.Directory _child;

        public MultiIndexLockFactory(Lucene.Net.Store.Directory master, Lucene.Net.Store.Directory child)
        {
            if (master == null) throw new ArgumentNullException("master");
            if (child == null) throw new ArgumentNullException("child");
            _master = master;
            _child = child;
        }

        public override Lock MakeLock(string lockName)
        {
            return new MultiIndexLock(_master.GetLockFactory().MakeLock(lockName), _child.GetLockFactory().MakeLock(lockName));
        }

        public override void ClearLock(string lockName)
        {
            _master.GetLockFactory().ClearLock(lockName);
            _child.GetLockFactory().ClearLock(lockName);
        }
    }
}