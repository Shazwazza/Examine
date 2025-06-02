using System;
using System.Collections.Concurrent;
using Examine.Lucene.Providers;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    /// <inheritdoc/>
    public abstract class DirectoryFactoryBase : IDirectoryFactory
    {
        Directory IDirectoryFactory.CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
            => CreateDirectory(luceneIndex, forceUnlock);

        Directory IDirectoryFactory.CreateTaxonomyDirectory(LuceneIndex luceneIndex, bool forceUnlock)
            => _createdDirectories.GetOrAdd(
                luceneIndex.Name + "_taxonomy",
                s => CreateTaxonomyDirectory(luceneIndex, forceUnlock));

        /// <inheritdoc/>
        protected abstract Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock);

        /// <inheritdoc/>
        protected virtual Directory CreateTaxonomyDirectory(LuceneIndex luceneIndex, bool forceUnlock) => throw new NotSupportedException("Directory Factory does not implement CreateTaxonomyDirectory ");

        /// <summary>
        /// Disposes the instance
        /// </summary>
        /// <param name="disposing">If the call is coming from the Dispose method</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose() =>
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
    }
}
