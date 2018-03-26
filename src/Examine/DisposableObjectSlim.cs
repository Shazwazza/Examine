using System;
using System.Threading;

namespace Examine
{
    /// <summary>
    /// This should not be use if there are unmanaged resources to be disposed, use DisposableObject instead
    /// </summary>
    internal abstract class DisposableObjectSlim : IDisposable
    {
        private bool _disposed;
        private readonly object _locko = new object();

        // gets a value indicating whether this instance is disposed.
        // for internal tests only (not thread safe)
        internal bool Disposed => _disposed;

        // implements IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            lock (_locko)
            {
                if (_disposed) return;
                _disposed = true;
            }

            if (disposing)
                DisposeResources();
        }

        protected abstract void DisposeResources();
    }
}