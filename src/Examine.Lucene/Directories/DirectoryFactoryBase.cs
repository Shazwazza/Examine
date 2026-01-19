using Examine.Lucene.Providers;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    /// <summary>
    /// Provides a base class for creating and managing Lucene.NET directory instances.
    /// </summary>
    public abstract class DirectoryFactoryBase : IDirectoryFactory
    {
        Directory IDirectoryFactory.CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
            => CreateDirectory(luceneIndex, forceUnlock);

        Directory? IDirectoryFactory.CreateTaxonomyDirectory(LuceneIndex luceneIndex, bool forceUnlock)
            => CreateTaxonomyDirectory(luceneIndex, forceUnlock);

        /// <inheritdoc/>
        protected abstract Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock);

        /// <inheritdoc/>
        protected abstract Directory? CreateTaxonomyDirectory(LuceneIndex luceneIndex, bool forceUnlock);

        /// <summary>
        /// Releases the unmanaged resources used by the object and optionally releases the managed resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <inheritdoc/>
        public void Dispose() =>
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
    }
}
