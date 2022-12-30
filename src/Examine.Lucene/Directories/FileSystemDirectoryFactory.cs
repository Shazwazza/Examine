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

        protected override Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
        {
            var path = Path.Combine(_baseDir.FullName, luceneIndex.Name);
            var luceneIndexFolder = new DirectoryInfo(path);

            var dir = FSDirectory.Open(luceneIndexFolder, LockFactory.GetLockFactory(luceneIndexFolder));
            if (forceUnlock)
            {
                IndexWriter.Unlock(dir);
            }
            return dir;
        }

        protected override Directory CreateTaxonomyDirectory(LuceneIndex luceneIndex, bool forceUnlock)
        {
            var path = Path.Combine(_baseDir.FullName, luceneIndex.Name,"taxonomy");
            var luceneIndexFolder = new DirectoryInfo(path);

            var dir = FSDirectory.Open(luceneIndexFolder, LockFactory.GetLockFactory(luceneIndexFolder));
            if (forceUnlock)
            {
                IndexWriter.Unlock(dir);
            }
            return dir;
        }
    }
}
