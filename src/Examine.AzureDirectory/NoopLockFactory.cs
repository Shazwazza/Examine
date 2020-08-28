using Lucene.Net.Store;

namespace Examine.AzureDirectory
{
    /// <summary>
    /// A lock factory used for readonly instances that do no writing
    /// </summary>
    public class NoopLockFactory : LockFactory
    {
        private static readonly NoopLock _noopLock = new NoopLock();

        public override void ClearLock(string lockName)
        {
        }

        public override Lock MakeLock(string lockName) => _noopLock;

        public class NoopLock : Lock
        {
            public override bool IsLocked() => false;

            public override bool Obtain() => true;

            public override void Release()
            {
            }
        }
    }

}
