
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
    /// directory, then creates a lucene Directory based on that replicated index.
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
        private ExamineReplicator _replicator;

        public SyncedFileSystemDirectoryFactory(
            DirectoryInfo localDir,
            DirectoryInfo mainDir,
            ILockFactory lockFactory,
            ILoggerFactory loggerFactory)
            : this(localDir, mainDir, lockFactory, loggerFactory, false)
        {
        }

        public SyncedFileSystemDirectoryFactory(
            DirectoryInfo localDir,
            DirectoryInfo mainDir,
            ILockFactory lockFactory,
            ILoggerFactory loggerFactory,
            bool tryFixMainIndexIfCorrupt)
            : base(mainDir, lockFactory)
        {
            _localDir = localDir;
            _mainDir = mainDir;
            _loggerFactory = loggerFactory;
            _tryFixMainIndexIfCorrupt = tryFixMainIndexIfCorrupt;
            _logger = _loggerFactory.CreateLogger<SyncedFileSystemDirectoryFactory>();
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
                mainResult = CheckIndexHealthAndFix(mainLuceneDir, luceneIndex.Name, _tryFixMainIndexIfCorrupt);
            }

            // the main index is/was unhealthy or missing, lets check the local index if it exists
            if (localIndexExists && (!mainIndexExists || mainResult.HasFlag(CreateResult.NotClean) || mainResult.HasFlag(CreateResult.MissingSegments)))
            {
                var localResult = CheckIndexHealthAndFix(localLuceneDir, luceneIndex.Name, false);

                if (localResult == CreateResult.Init)
                {
                    // it was read successfully, we can sync back to main
                    localResult |= TryGetIndexWriter(OpenMode.APPEND, localLuceneDir, false, luceneIndex.Name, out var indexWriter);
                    using (indexWriter)
                    {
                        if (localResult.HasFlag(CreateResult.OpenedSuccessfully))
                        {
                            SyncIndex(indexWriter, true, luceneIndex.Name, mainLuceneIndexFolder, tempDir);
                            mainResult |= CreateResult.SyncedFromLocal;
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

                mainResult |= TryGetIndexWriter(openMode, mainLuceneDir, true, luceneIndex.Name, out var indexWriter);
                using (indexWriter)
                {
                    if (!mainResult.HasFlag(CreateResult.SyncedFromLocal))
                    {
                        SyncIndex(indexWriter, forceUnlock, luceneIndex.Name, localLuceneIndexFolder, tempDir);
                    }
                }
            }

            // now create the replicator that will copy from local to main on schedule
            _replicator = new ExamineReplicator(_loggerFactory, luceneIndex, mainLuceneDir, tempDir);

            if (forceUnlock)
            {
                IndexWriter.Unlock(localLuceneDir);
            }

            // Start replicating back to main
            _replicator.StartIndexReplicationOnSchedule(1000);

            directory = localLuceneDir;

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

        protected override Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
        {
            _ = TryCreateDirectory(luceneIndex, forceUnlock, out var directory);
            return directory;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _replicator?.Dispose();
            }
        }

        private CreateResult TryGetIndexWriter(
            OpenMode openMode,
            Directory luceneDirectory,
            bool createNewIfCorrupt,
            string indexName,
            out IndexWriter indexWriter)
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
                    return CreateResult.CorruptCreatedNew;
                }
            }
            catch (Exception ex)
            {
                if (createNewIfCorrupt)
                {
                    // Index is corrupted, typically this will be FileNotFoundException
                    _logger.LogError(ex, "{IndexName} index is corrupt, a new one will be created", indexName);

                    indexWriter = GetIndexWriter(luceneDirectory, OpenMode.CREATE);
                }
                else
                {
                    indexWriter = null;
                }

                return CreateResult.CorruptCreatedNew;
            }
        }

        private void SyncIndex(IndexWriter sourceIndexWriter, bool forceUnlock, string indexName, DirectoryInfo destinationDirectory, DirectoryInfo tempDir)
        {
            // First, we need to clear the main index. If for some reason it is at the same revision, the syncing won't do anything.
            if (destinationDirectory.Exists)
            {
                foreach (var file in destinationDirectory.EnumerateFiles())
                {
                    file.Delete();
                }
            }

            using (var sourceIndex = new LuceneIndex(_loggerFactory, indexName, new TempOptions(), sourceIndexWriter))
            using (var destinationLuceneDirectory = FSDirectory.Open(destinationDirectory, LockFactory.GetLockFactory(destinationDirectory)))
            using (var replicator = new ExamineReplicator(_loggerFactory, sourceIndex, destinationLuceneDirectory, tempDir))
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
                _logger.LogWarning("{IndexName} index is missing segments, it will be deleted.", indexName);
                result = CreateResult.MissingSegments;
            }
            else if (!status.Clean)
            {
                _logger.LogWarning("Checked main index and it is not clean, attempting to fix {IndexName}. {DocumentsLost} documents will be lost.", indexName, status.TotLoseDocCount);
                result = CreateResult.NotClean;

                if (doFix)
                {
                    try
                    {
                        checker.FixIndex(status);
                        status = checker.DoCheckIndex();

                        if (!status.Clean)
                        {
                            _logger.LogError("{IndexName} index could not be fixed, it will be deleted.", indexName);
                            result |= CreateResult.NotFixed;
                        }
                        else
                        {
                            _logger.LogInformation("Index {IndexName} fixed. {DocumentsLost} documents were lost.", indexName, status.TotLoseDocCount);
                            result |= CreateResult.Fixed;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{IndexName} index could not be fixed, it will be deleted.", indexName);
                        result |= CreateResult.ExceptionNotFixed;
                    }
                }
            }
            else
            {
                _logger.LogInformation("Checked main index {IndexName} and it is clean.", indexName);
            }

            return result;
        }

        private IndexWriter GetIndexWriter(Directory mainDir, OpenMode openMode)
        {
            var indexWriter = new IndexWriter(
                mainDir,
                new IndexWriterConfig(
                    LuceneInfo.CurrentVersion,
                    new StandardAnalyzer(LuceneInfo.CurrentVersion))
                {
                    OpenMode = openMode,
                    IndexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy())
                });

            return indexWriter;
        }

        private class TempOptions : IOptionsMonitor<LuceneDirectoryIndexOptions>
        {
            public LuceneDirectoryIndexOptions CurrentValue => new LuceneDirectoryIndexOptions();
            public LuceneDirectoryIndexOptions Get(string name) => CurrentValue;

            public IDisposable OnChange(Action<LuceneDirectoryIndexOptions, string> listener) => throw new NotImplementedException();
        }

    }
}
