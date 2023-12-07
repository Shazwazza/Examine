using System.IO;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    /// <summary>
    /// Represents a directory factory for creating file system directories
    /// </summary>
    public class FileSystemDirectoryFactory : DirectoryFactoryBase
    {
        private readonly DirectoryInfo _baseDir;

        /// <summary>
        /// Creates an instance of <see cref="FileSystemDirectoryFactory"/>
        /// </summary>
        /// <param name="baseDir">The base directory</param>
        /// <param name="lockFactory">The lock factory</param>
        public FileSystemDirectoryFactory(DirectoryInfo baseDir, ILockFactory lockFactory)
        {
            _baseDir = baseDir;
            LockFactory = lockFactory;
        }

        /// <summary>
        /// The factory for creating locks
        /// </summary>
        public ILockFactory LockFactory { get; }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
