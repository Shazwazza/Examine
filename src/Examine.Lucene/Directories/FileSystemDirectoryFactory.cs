using System.IO;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.Extensions.Options;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    public class FileSystemDirectoryFactory : DirectoryFactoryBase
    {
        private readonly DirectoryInfo _baseDir;
        private readonly IOptionsMonitor<LuceneDirectoryIndexOptions> _indexOptions;

        public FileSystemDirectoryFactory(
            DirectoryInfo baseDir,
            ILockFactory lockFactory,
            IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions)
        {
            _baseDir = baseDir;
            LockFactory = lockFactory;
            _indexOptions = indexOptions;
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

            var options = _indexOptions.GetNamedOptions(luceneIndex.Name);
            if (options.NrtEnabled)
            {
                return new NRTCachingDirectory(dir, 5.0, 60.0);
            }
            else
            {
                return dir;
            }
        }
    }
}
