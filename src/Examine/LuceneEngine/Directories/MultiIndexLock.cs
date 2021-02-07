using System.Security;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Directories
{
    /// <summary>
    /// Lock that wraps multiple locks
    /// </summary>
    
    internal class MultiIndexLock : Lock
    {
        private readonly Lock _dirMaster;
        private readonly Lock _dirChild;
        
        public MultiIndexLock(Lock dirMaster, Lock dirChild)
        {
            _dirMaster = dirMaster;
            _dirChild = dirChild;
        }

        /// <summary>
        /// Attempts to obtain exclusive access and immediately return
        ///             upon success or failure.
        /// </summary>
        /// <returns>
        /// true iff exclusive access is obtained
        /// </returns>
        
        public override bool Obtain()
        {
            var master = _dirMaster.Obtain();
            if (!master) return false;
            var child = _dirChild.Obtain();
            return child;
        }

        
        public override bool Obtain(long lockWaitTimeout)
        {
            var master = _dirMaster.Obtain(lockWaitTimeout);
            if (!master) return false;
            var child = _dirChild.Obtain(lockWaitTimeout);
            return child;
        }

        /// <summary>
        /// Releases exclusive access. 
        /// </summary>
        
        public override void Release()
        {
            var isChild = false;
            try
            {
                //try to release master
                _dirMaster.Release();

                //if that succeeds try to release child
                isChild = true;
                _dirChild.Release();
            }
            catch (System.Exception)
            {
                //if an error occurs above for the master still attempt to release child
                if (!isChild)
                    _dirChild.Release();

                throw;
            }
            
        }

        /// <summary>
        /// Returns true if the resource is currently locked.  Note that one must
        ///             still call <see cref="M:Lucene.Net.Store.Lock.Obtain"/> before using the resource. 
        /// </summary>
        
        public override bool IsLocked()
        {
            return _dirMaster.IsLocked() || _dirChild.IsLocked();
        }
    }
}