using System;
using System.IO;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.Extensions.Options;
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
        public FileSystemDirectoryFactory(
            DirectoryInfo baseDir,
            ILockFactory lockFactory,
            IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions)
        {
            _baseDir = baseDir;
            LockFactory = lockFactory;
            IndexOptions = indexOptions;
        }

        /// <summary>
        /// The factory for creating locks
        /// </summary>
        public ILockFactory LockFactory { get; }

        /// <summary>
        /// Provides access to index options for Lucene directories.
        /// </summary>
        protected IOptionsMonitor<LuceneDirectoryIndexOptions> IndexOptions { get; }

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

        /// <inheritdoc/>
        protected override Directory CreateTaxonomyDirectory(LuceneIndex luceneIndex, bool forceUnlock)
        {
            var path = Path.Combine(_baseDir.FullName, luceneIndex.Name, "taxonomy");
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
