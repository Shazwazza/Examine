using System;
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

        [Obsolete("Use ctor with all dependencies")]
        public FileSystemDirectoryFactory(
            DirectoryInfo baseDir,
            ILockFactory lockFactory)
            : this (baseDir, lockFactory, new FakeLuceneDirectoryIndexOptionsOptionsMonitor())
        {
        }

        public FileSystemDirectoryFactory(
            DirectoryInfo baseDir,
            ILockFactory lockFactory,
            IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions)
        {
            _baseDir = baseDir;
            LockFactory = lockFactory;
            IndexOptions = indexOptions;
        }

        public ILockFactory LockFactory { get; }

        protected IOptionsMonitor<LuceneDirectoryIndexOptions> IndexOptions { get; }

        protected override Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
        {
            var path = Path.Combine(_baseDir.FullName, luceneIndex.Name);
            var luceneIndexFolder = new DirectoryInfo(path);

            var dir = FSDirectory.Open(luceneIndexFolder, LockFactory.GetLockFactory(luceneIndexFolder));
            if (forceUnlock)
            {
                IndexWriter.Unlock(dir);
            }

            var options = IndexOptions.GetNamedOptions(luceneIndex.Name);
            if (options.NrtEnabled)
            {
                return new NRTCachingDirectory(dir, options.NrtCacheMaxMergeSizeMB, options.NrtCacheMaxCachedMB);
            }
            else
            {
                return dir;
            }
        }
    }
}
