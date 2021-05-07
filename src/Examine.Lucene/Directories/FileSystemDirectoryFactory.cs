using System.IO;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    public class FileSystemDirectoryFactory : IDirectoryFactory
    {
        private readonly DirectoryInfo _baseDir;
        private readonly ILockFactory _lockFactory;

        public FileSystemDirectoryFactory(DirectoryInfo baseDir, ILockFactory lockFactory)
        {
            _baseDir = baseDir;
            _lockFactory = lockFactory;
        }

        public virtual Directory CreateDirectory(string indexName)
        {
            var path = Path.Combine(_baseDir.FullName, indexName);
            var luceneIndexFolder = new DirectoryInfo(path);

            return new SimpleFSDirectory(luceneIndexFolder, _lockFactory.GetLockFactory(luceneIndexFolder));
        }
    }
}
