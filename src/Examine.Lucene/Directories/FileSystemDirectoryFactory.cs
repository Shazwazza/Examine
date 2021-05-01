using System.IO;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    public class FileSystemDirectoryFactory : DirectoryFactory
    {
        private readonly ILockFactory _lockFactory;

        public FileSystemDirectoryFactory(ILockFactory lockFactory) => _lockFactory = lockFactory;

        public override Directory CreateDirectory(DirectoryInfo luceneIndexFolder)
            => new SimpleFSDirectory(luceneIndexFolder, _lockFactory.GetLockFactory(luceneIndexFolder));
    }
}
