using System;
using System.IO;
using Examine.Lucene.Providers;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    /// <summary>
    /// A directory factory that replicates the index from main storage on initialization to another
    /// directory, then creates a Lucene Directory based on that replicated index.
    /// </summary>
    /// <remarks>
    /// A replication thread is spawned to then replicate the local index back to the main storage location.
    /// By default, Examine configures the local directory to be the %temp% folder.
    /// This also checks if the main/local storage indexes are healthy and syncs/removes accordingly.
    /// </remarks>
    public class SyncedFileSystemDirectoryFactory : FileSystemDirectoryFactory
    {
        private readonly DirectoryInfo _localDir;
        private readonly DirectoryInfo _mainDir;
        private readonly ILoggerFactory _loggerFactory;
        private readonly bool _tryFixMainIndexIfCorrupt;
        private readonly ILogger<SyncedFileSystemDirectoryFactory> _logger;
        private readonly ILogger<ExamineReplicator> _replicatorLogger;
        private readonly ILogger<LoggingReplicationClient> _clientLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncedFileSystemDirectoryFactory"/> class.
        /// </summary>
        /// <param name="localDir">The local directory where the index will be replicated.</param>
        /// <param name="mainDir">The main directory where the index is stored.</param>
        /// <param name="lockFactory">The lock factory used for managing index locks.</param>
        /// <param name="loggerFactory">The logger factory used for creating loggers.</param>
        /// <param name="indexOptions">The options monitor for Lucene directory index options.</param>
        public SyncedFileSystemDirectoryFactory(
            DirectoryInfo localDir,
            DirectoryInfo mainDir,
            ILockFactory lockFactory,
            ILoggerFactory loggerFactory,
            IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions)
            : this(localDir, mainDir, lockFactory, loggerFactory, indexOptions, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncedFileSystemDirectoryFactory"/> class.
        /// </summary>
        /// <param name="localDir">The local directory where the index will be replicated.</param>
        /// <param name="mainDir">The main directory where the index is stored.</param>
        /// <param name="lockFactory">The lock factory used for managing index locks.</param>
        /// <param name="loggerFactory">The logger factory used for creating loggers.</param>
        /// <param name="indexOptions">The options monitor for Lucene directory index options.</param>
        /// <param name="tryFixMainIndexIfCorrupt">Indicates whether to attempt fixing the main index if it is corrupt.</param>
        public SyncedFileSystemDirectoryFactory(
            DirectoryInfo localDir,
            DirectoryInfo mainDir,
            ILockFactory lockFactory,
            ILoggerFactory loggerFactory,
            IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions,
            bool tryFixMainIndexIfCorrupt)
            : base(mainDir, lockFactory, indexOptions)
        {
            _localDir = localDir;
            _mainDir = mainDir;
            _loggerFactory = loggerFactory;
            _tryFixMainIndexIfCorrupt = tryFixMainIndexIfCorrupt;
            _logger = _loggerFactory.CreateLogger<SyncedFileSystemDirectoryFactory>();
            _replicatorLogger = _loggerFactory.CreateLogger<ExamineReplicator>();
            _clientLogger = _loggerFactory.CreateLogger<LoggingReplicationClient>();
        }

        internal CreateResult TryCreateDirectory(LuceneIndex luceneIndex, bool forceUnlock, out Directory directory)
        {
            var mainPath = Path.Combine(_mainDir.FullName, luceneIndex.Name);
            var mainLuceneIndexFolder = new DirectoryInfo(mainPath);

            var localPath = Path.Combine(_localDir.FullName, luceneIndex.Name);
            var localLuceneIndexFolder = new DirectoryInfo(localPath);

            // used by the replicator, will be a short lived directory for each synced revision and deleted when finished.
            var tempDir = new DirectoryInfo(Path.Combine(_localDir.FullName, "Rep", Guid.NewGuid().ToString("N")));

            var mainLuceneDir = base.CreateDirectory(luceneIndex, forceUnlock);
            var localLuceneDir = FSDirectory.Open(
                localLuceneIndexFolder,
                LockFactory.GetLockFactory(localLuceneIndexFolder));

            var mainIndexExists = DirectoryReader.IndexExists(mainLuceneDir);
            var localIndexExists = DirectoryReader.IndexExists(localLuceneDir);

            var mainResult = CreateResult.Init;

            if (mainIndexExists)
            {
                mainResult = CheckIndexHealthAndFix(mainLuceneDir, mainLuceneIndexFolder, luceneIndex.Name, _tryFixMainIndexIfCorrupt);
            }

            // the main index is/was unhealthy or missing, lets check the local index if it exists
            if (localIndexExists && (!mainIndexExists || mainResult.HasFlag(CreateResult.NotClean) || mainResult.HasFlag(CreateResult.MissingSegments)))
            {
                // TODO: add details here and more below too

                var localResult = CheckIndexHealthAndFix(localLuceneDir, localLuceneIndexFolder, luceneIndex.Name, false);

                if (localResult == CreateResult.Init)
                {
                    // it was read successfully, we can sync back to main
                    localResult |= TryGetIndexWriter(OpenMode.APPEND, localLuceneDir, localLuceneIndexFolder, false, luceneIndex.Name, out var indexWriter);
                    if (localResult.HasFlag(CreateResult.OpenedSuccessfully))
                    {
                        using (indexWriter!)
                        {
                            SyncIndex(indexWriter!, true, luceneIndex.Name, mainLuceneIndexFolder, tempDir);
                            mainResult |= CreateResult.SyncedFromLocal;
                            // we need to check the main index again, as it may have been fixed by the sync
                            mainIndexExists = DirectoryReader.IndexExists(mainLuceneDir);
                        }
                    }
                }
            }

            if (mainIndexExists)
            {
                // when the lucene directory is going to be created, we'll sync from main storage to local
                // storage before any index/writer is opened.

                var openMode = mainResult == CreateResult.Init || mainResult.HasFlag(CreateResult.Fixed) || mainResult.HasFlag(CreateResult.SyncedFromLocal)
                            ? OpenMode.APPEND
                            : OpenMode.CREATE;

                mainResult |= TryGetIndexWriter(openMode, mainLuceneDir, mainLuceneIndexFolder, true, luceneIndex.Name, out var indexWriter);
                if (indexWriter is not null)
                {
                    using (indexWriter)
                    {
                        if (!mainResult.HasFlag(CreateResult.SyncedFromLocal))
                        {
                            SyncIndex(indexWriter, forceUnlock, luceneIndex.Name, localLuceneIndexFolder, tempDir);
                        }
                    }
                }
            }

            if (forceUnlock)
            {
                IndexWriter.Unlock(localLuceneDir);
            }

            Directory luceneDir;

            var options = IndexOptions.GetNamedOptions(luceneIndex.Name);
            if (options.NrtEnabled)
            {
                luceneDir = new NRTCachingDirectory(localLuceneDir, options.NrtCacheMaxMergeSizeMB, options.NrtCacheMaxCachedMB);
            }
            else
            {
                luceneDir = localLuceneDir;
            }

            directory = new SyncedFileSystemDirectory(_replicatorLogger, _clientLogger, luceneDir, mainLuceneDir, luceneIndex, tempDir);

            return mainResult;
        }

        [Flags]
        internal enum CreateResult
        {
            Init = 0,
            MissingSegments = 1,
            NotClean = 2,
            Fixed = 4,
            NotFixed = 8,
            ExceptionNotFixed = 16,
            CorruptCreatedNew = 32,
            OpenedSuccessfully = 64,
            SyncedFromLocal = 128
        }

        /// <inheritdoc />
        protected override Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
        {
            _ = TryCreateDirectory(luceneIndex, forceUnlock, out var directory);
            return directory;
        }

        private CreateResult TryGetIndexWriter(
            OpenMode openMode,
            Directory luceneDirectory,
            DirectoryInfo directoryInfo,
            bool createNewIfCorrupt,
            string indexName,
            out IndexWriter? indexWriter)
        {
            try
            {
                indexWriter = GetIndexWriter(luceneDirectory, openMode);

                if (openMode == OpenMode.APPEND)
                {
                    return CreateResult.OpenedSuccessfully;
                }
                else
                {
                    // Required to remove old index files which can be problematic
                    // if they remain in the index folder when replication is attempted.
                    indexWriter.Commit();
                    indexWriter.WaitForMerges();

                    return CreateResult.CorruptCreatedNew;
                }
            }
            catch (Exception ex)
            {
                if (createNewIfCorrupt)
                {
                    // Index is corrupted, typically this will be FileNotFoundException or CorruptIndexException
                    _logger.LogError(ex, "{IndexName} at {IndexPath} index is corrupt, a new one will be created", indexName, directoryInfo.FullName);

                    // Totally clear all files in the directory
                    ClearDirectory(directoryInfo);

                    indexWriter = GetIndexWriter(luceneDirectory, OpenMode.CREATE);
                }
                else
                {
                    indexWriter = null;
                }

                return CreateResult.CorruptCreatedNew;
            }
        }

        private void ClearDirectory(DirectoryInfo directoryInfo)
        {
            if (directoryInfo.Exists)
            {
                foreach (var file in directoryInfo.EnumerateFiles())
                {
                    file.Delete();
                }
            }
        }

        private void SyncIndex(IndexWriter sourceIndexWriter, bool forceUnlock, string indexName, DirectoryInfo destinationDirectory, DirectoryInfo tempDir)
        {
            // First, we need to clear the main index. If for some reason it is at the same revision, the syncing won't do anything.
            ClearDirectory(destinationDirectory);

            using (var sourceIndex = new LuceneIndex(_loggerFactory, indexName, new TempOptions(), sourceIndexWriter))
            using (var destinationLuceneDirectory = FSDirectory.Open(destinationDirectory, LockFactory.GetLockFactory(destinationDirectory)))
            using (var replicator = new ExamineReplicator(_replicatorLogger, _clientLogger, sourceIndex, sourceIndexWriter.Directory, destinationLuceneDirectory, tempDir))
            {
                if (forceUnlock)
                {
                    IndexWriter.Unlock(destinationLuceneDirectory);
                }

                // replicate locally.
                replicator.ReplicateIndex();
            }
        }

        private CreateResult CheckIndexHealthAndFix(
            Directory luceneDir,
            DirectoryInfo directoryInfo,
            string indexName,
            bool doFix)
        {
            using var writer = new StringWriter();
            var result = CreateResult.Init;

            var checker = new CheckIndex(luceneDir)
            {
                // Redirect the logging output of the checker
                InfoStream = writer
            };

            var status = checker.DoCheckIndex();
            writer.Flush();

            _logger.LogDebug("{IndexName} health check report {IndexReport}", indexName, writer.ToString());

            if (status.MissingSegments)
            {
                _logger.LogWarning("{IndexName} index at {IndexPath} is missing segments, it will be deleted.", indexName, directoryInfo.FullName);
                result = CreateResult.MissingSegments;
            }
            else if (!status.Clean)
            {
                _logger.LogWarning("Checked index {IndexName} at {IndexPath} and it is not clean.", indexName, directoryInfo.FullName);
                result = CreateResult.NotClean;

                if (doFix)
                {
                    _logger.LogWarning("Attempting to fix {IndexName} at {IndexPath}. {DocumentsLost} documents will be lost.", indexName, status.TotLoseDocCount, directoryInfo.FullName);

                    try
                    {
                        checker.FixIndex(status);
                        status = checker.DoCheckIndex();

                        if (!status.Clean)
                        {
                            _logger.LogError("{IndexName} index at {IndexPath} could not be fixed, it will be deleted.", indexName, directoryInfo.FullName);
                            result |= CreateResult.NotFixed;
                        }
                        else
                        {
                            _logger.LogInformation("Index {IndexName} at {IndexPath} fixed. {DocumentsLost} documents were lost.", indexName, status.TotLoseDocCount, directoryInfo.FullName);
                            result |= CreateResult.Fixed;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{IndexName} index at {IndexPath} could not be fixed, it will be deleted.", indexName, directoryInfo.FullName);
                        result |= CreateResult.ExceptionNotFixed;
                    }
                }
            }
            else
            {
                _logger.LogInformation("Checked index {IndexName} at {IndexPath} and it is clean.", indexName, directoryInfo.FullName);
            }

            return result;
        }

        private static IndexWriter GetIndexWriter(Directory mainDir, OpenMode openMode)
        {
            var indexWriter = new IndexWriter(
                mainDir,
                new IndexWriterConfig(
                    LuceneInfo.CurrentVersion,
                    new StandardAnalyzer(LuceneInfo.CurrentVersion))
                {
                    OpenMode = openMode,
                    IndexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy()),
                    MergePolicy = new TieredMergePolicy()
                });

            return indexWriter;
        }

        private class TempOptions : IOptionsMonitor<LuceneDirectoryIndexOptions>
        {
            public LuceneDirectoryIndexOptions CurrentValue => new LuceneDirectoryIndexOptions();

            public LuceneDirectoryIndexOptions Get(string? name) => CurrentValue;

            public IDisposable OnChange(Action<LuceneDirectoryIndexOptions, string> listener) => throw new NotImplementedException();
        }

    }
}
