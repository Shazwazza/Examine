using System;
using System.Collections.Concurrent;
using Lucene.Net.Store;

namespace Examine.Lucene.Suggest.Directories
{
    /// <summary>
    /// Base class for Lucene Suggester Directory Factory
    /// </summary>
    public abstract class SuggesterDirectoryFactoryBase : ISuggesterDirectoryFactory
    {
        private readonly ConcurrentDictionary<string, Directory> _createdDirectories = new ConcurrentDictionary<string, Directory>();
        private bool _disposedValue;

        /// <inheritdoc/>
        Directory ISuggesterDirectoryFactory.CreateDirectory(string name, bool forceUnlock)
            => _createdDirectories.GetOrAdd(
               name,
                s => CreateDirectory(name, forceUnlock));

        /// <inheritdoc/>
        protected abstract Directory CreateDirectory(string name, bool forceUnlock);

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (Directory d in _createdDirectories.Values)
                    {
                        d.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
