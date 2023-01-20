using System.IO;
using Examine.Lucene.Directories;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Suggest.Directories
{
    public class FileSystemSuggesterDirectoryFactory : SuggesterDirectoryFactoryBase
    {
        private readonly DirectoryInfo _baseDir;

        public FileSystemSuggesterDirectoryFactory(DirectoryInfo baseDir, ILockFactory lockFactory)
        {
            _baseDir = baseDir;
            LockFactory = lockFactory;
        }

        public ILockFactory LockFactory { get; }

        protected override Directory CreateDirectory(string name, bool forceUnlock)
        {
            var path = Path.Combine(_baseDir.FullName, name);
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
