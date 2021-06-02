using System.Collections.Concurrent;
using Examine.Lucene.Providers;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    public abstract class DirectoryFactoryBase : IDirectoryFactory
    {
        private readonly ConcurrentDictionary<string, Directory> _createdDirectories = new ConcurrentDictionary<string, Directory>();
        private bool _disposedValue;

        Directory IDirectoryFactory.CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
            => _createdDirectories.GetOrAdd(
                luceneIndex.Name,
                s => CreateDirectory(luceneIndex, forceUnlock));

        protected abstract Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock);

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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
