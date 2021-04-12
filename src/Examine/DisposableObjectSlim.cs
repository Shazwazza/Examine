using System;

namespace Examine
{   

    /// <summary>
    /// This should not be use if there are unmanaged resources to be disposed, use DisposableObject instead
    /// </summary>
    internal abstract class DisposableObjectSlim : IDisposable
    {
        private readonly object _locko = new object();

        // gets a value indicating whether this instance is disposed.
        // for internal tests only (not thread safe)
        protected bool Disposed { get; private set; }

        // implements IDisposable
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            lock (_locko)
            {
                if (Disposed) return;
                Disposed = true;
            }

            if (disposing)
                DisposeResources();
        }

        protected abstract void DisposeResources();
    }
}