using System.IO;
using System.Threading;
using Examine.Lucene.Providers;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{

    public class FileSystemDirectoryFactory : IDirectoryFactory
    {
        private readonly DirectoryInfo _baseDir;
        private bool _disposedValue;
        private Directory _directory;

        public FileSystemDirectoryFactory(DirectoryInfo baseDir, ILockFactory lockFactory)
        {
            _baseDir = baseDir;
            LockFactory = lockFactory;
        }

        public ILockFactory LockFactory { get; }

        public virtual Directory CreateDirectory(LuceneIndex index)
            => LazyInitializer.EnsureInitialized(ref _directory, () =>
            {
                var path = Path.Combine(_baseDir.FullName, index.Name);
                var luceneIndexFolder = new DirectoryInfo(path);

                return new SimpleFSDirectory(luceneIndexFolder, LockFactory.GetLockFactory(luceneIndexFolder));
            });

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _directory?.Dispose();
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
