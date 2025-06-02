using System;
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
            => CreateTaxonomyDirectory(luceneIndex, forceUnlock);

        /// <inheritdoc/>
        protected abstract Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock);

        /// <inheritdoc/>
        protected virtual Directory CreateTaxonomyDirectory(LuceneIndex luceneIndex, bool forceUnlock) => throw new NotSupportedException("Directory Factory does not implement CreateTaxonomyDirectory ");
    }
}
