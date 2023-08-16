using System;

namespace Examine
{   

    /// <summary>
    /// This should not be use if there are unmanaged resources to be disposed, use DisposableObject instead
    /// </summary>
    public abstract class DisposableObjectSlim : IDisposable
    {
        private readonly object _locko = new object();

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// for internal tests only (not thread safe)
        /// </summary>
        protected bool Disposed { get; private set; }

        /// <inheritdoc/>
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

        /// <summary>
        /// Used to dispose resources
        /// </summary>
        protected abstract void DisposeResources();
    }
}
