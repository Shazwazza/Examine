using System.IO;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    public class FileSystemDirectoryFactory : DirectoryFactoryBase
    {
        private readonly DirectoryInfo _baseDir;

        public FileSystemDirectoryFactory(DirectoryInfo baseDir, ILockFactory lockFactory)
        {
            _baseDir = baseDir;
            LockFactory = lockFactory;
        }

        public ILockFactory LockFactory { get; }

        protected override Directory CreateDirectory(LuceneIndex index, bool forceUnlock)
        {
            var path = Path.Combine(_baseDir.FullName, index.Name);
            var luceneIndexFolder = new DirectoryInfo(path);

            var dir = new SimpleFSDirectory(luceneIndexFolder, LockFactory.GetLockFactory(luceneIndexFolder));
            if (forceUnlock)
            {
                IndexWriter.Unlock(dir);
            }
            return dir;
        }
    }
}
