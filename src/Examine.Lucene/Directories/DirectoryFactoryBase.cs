using System.Collections.Concurrent;
using Examine.Lucene.Providers;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    public abstract class DirectoryFactoryBase : IDirectoryFactory
    {
        Directory IDirectoryFactory.CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
            => CreateDirectory(luceneIndex, forceUnlock);

        protected abstract Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock);

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose() =>
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
    }
}
