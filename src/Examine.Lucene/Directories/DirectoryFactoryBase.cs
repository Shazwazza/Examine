using System;
using System.Collections.Concurrent;
using Examine.Lucene.Providers;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    /// <inheritdoc/>
    public abstract class DirectoryFactoryBase : IDirectoryFactory
    {
        private readonly ConcurrentDictionary<string, Directory> _createdDirectories = new ConcurrentDictionary<string, Directory>();
        private bool _disposedValue;

        Directory IDirectoryFactory.CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
            => _createdDirectories.GetOrAdd(
                luceneIndex.Name,
                s => CreateDirectory(luceneIndex, forceUnlock));

        Directory IDirectoryFactory.CreateTaxonomyDirectory(LuceneIndex luceneIndex, bool forceUnlock)
            => _createdDirectories.GetOrAdd(
                luceneIndex.Name + "_taxonomy",
                s => CreateTaxonomyDirectory(luceneIndex, forceUnlock));

        /// <inheritdoc/>
        protected abstract Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock);

        /// <inheritdoc/>
        protected virtual Directory CreateTaxonomyDirectory(LuceneIndex luceneIndex, bool forceUnlock) => throw new NotSupportedException("Directory Factory does not implement CreateTaxonomyDirectory ");

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {                    
                    foreach (var d in _createdDirectories.Values)
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
#pragma warning disable IDE0022 // Use expression body for method
            Dispose(disposing: true);
#pragma warning restore IDE0022 // Use expression body for method
        }
    }
}
