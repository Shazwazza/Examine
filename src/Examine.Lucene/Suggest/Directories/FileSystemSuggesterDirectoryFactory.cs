using System.IO;
using Examine.Lucene.Directories;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Suggest.Directories
{
    /// <summary>
    /// File System Suggester Directory Factory
    /// </summary>
    public class FileSystemSuggesterDirectoryFactory : SuggesterDirectoryFactoryBase
    {
        private readonly DirectoryInfo _baseDir;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseDir">Base Directory</param>
        /// <param name="lockFactory">Lucene Lock Factory</param>
        public FileSystemSuggesterDirectoryFactory(DirectoryInfo baseDir, ILockFactory lockFactory)
        {
            _baseDir = baseDir;
            LockFactory = lockFactory;
        }

        /// <summary>
        /// Lock Factory
        /// </summary>
        public ILockFactory LockFactory { get; }

        /// <inheritdoc/>
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
