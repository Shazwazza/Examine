
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
    /// directory, then creates a lucene Directory based on that replicated index. A replication thread
    /// is spawned to then replicate the local index back to the main storage location.
    /// </summary>
    /// <remarks>
    /// By default, Examine configures the local directory to be the %temp% folder.
    /// </remarks>
    public class SyncedFileSystemDirectoryFactory : FileSystemDirectoryFactory
    {
        private readonly DirectoryInfo _localDir;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<SyncedFileSystemDirectoryFactory> _logger;
        private ExamineReplicator _replicator;

        public SyncedFileSystemDirectoryFactory(
            DirectoryInfo localDir,
            DirectoryInfo mainDir,
            ILockFactory lockFactory,
            ILoggerFactory loggerFactory)
            : base(mainDir, lockFactory)
        {
            _localDir = localDir;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<SyncedFileSystemDirectoryFactory>();
        }

        internal CreateResult TryCreateDirectory(LuceneIndex luceneIndex, bool forceUnlock, out Directory directory)
        {
            var result = CreateResult.OpenedSuccessfully;

            var path = Path.Combine(_localDir.FullName, luceneIndex.Name);
            var localLuceneIndexFolder = new DirectoryInfo(path);

            var mainDir = base.CreateDirectory(luceneIndex, forceUnlock);

            using (var writer = new StringWriter())
            {
                var checker = new CheckIndex(mainDir)
                {
                    // Redirect the logging output of the checker
                    InfoStream = writer
                };

                var status = checker.DoCheckIndex();
                writer.Flush();

                _logger.LogDebug("{IndexName} health check report {IndexReport}", luceneIndex.Name, writer.ToString());

                if (status.MissingSegments)
                {
                    _logger.LogError("{IndexName} index is missing segments, it will be deleted.", luceneIndex.Name);
                    result = CreateResult.MissingSegments;
                }
                else if (!status.Clean)
                {
                    _logger.LogWarning("Checked main director index and it is not clean, attempting to fix {IndexName}. {DocumentsLost} documents will be lost.", luceneIndex.Name, status.TotLoseDocCount);
                    result = CreateResult.NotClean;

                    try
                    {
                        checker.FixIndex(status);
                        status = checker.DoCheckIndex();

                        if (!status.Clean)
                        {
                            _logger.LogError("{IndexName} index could not be fixed, it will be deleted.", luceneIndex.Name);
                            result |= CreateResult.NotFixed;
                        }
                        else
                        {
                            _logger.LogInformation("Index {IndexName} fixed. {DocumentsLost} documents were lost.", luceneIndex.Name, status.TotLoseDocCount);
                            result |= CreateResult.Fixed;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{IndexName} index could not be fixed, it will be deleted.", luceneIndex.Name);
                        result |= CreateResult.ExceptionNotFixed;
                    }
                }
                else
                {
                    _logger.LogInformation("Checked main director index {IndexName} and it is clean.", luceneIndex.Name);
                }
            }

            // used by the replicator, will be a short lived directory for each synced revision and deleted when finished.
            var tempDir = new DirectoryInfo(Path.Combine(_localDir.FullName, "Rep", Guid.NewGuid().ToString("N")));

            if (DirectoryReader.IndexExists(mainDir))
            {
                IndexWriter indexWriter;

                // when the lucene directory is going to be created, we'll sync from main storage to local
                // storage before any index/writer is opened.

                try
                {
                    var openMode = result == CreateResult.Init || result.HasFlag(CreateResult.Fixed)
                            ? OpenMode.APPEND
                            : OpenMode.CREATE;

                    indexWriter = GetIndexWriter(mainDir, openMode);

                    if (openMode == OpenMode.APPEND)
                    {
                        result |= CreateResult.OpenedSuccessfully;
                    }
                    else
                    {
                        result |= CreateResult.CorruptCreatedNew;
                    }
                }
                catch (Exception ex)
                {
                    // Index is corrupted, typically this will be FileNotFoundException
                    _logger.LogError(ex, "{IndexName} index is corrupt, a new one will be created", luceneIndex.Name);

                    indexWriter = GetIndexWriter(mainDir, OpenMode.CREATE);
                    result |= CreateResult.CorruptCreatedNew;
                }

                using (var tempMainIndexWriter = indexWriter)
                using (var tempMainIndex = new LuceneIndex(_loggerFactory, luceneIndex.Name, new TempOptions(), tempMainIndexWriter))
                using (var tempLocalDirectory = FSDirectory.Open(localLuceneIndexFolder, LockFactory.GetLockFactory(localLuceneIndexFolder)))
                using (var replicator = new ExamineReplicator(_loggerFactory, tempMainIndex, tempLocalDirectory, tempDir))
                {
                    if (forceUnlock)
                    {
                        IndexWriter.Unlock(tempLocalDirectory);
                    }

                    // replicate locally.
                    replicator.ReplicateIndex();
                }
            }

            // now create the replicator that will copy from local to main on schedule
            _replicator = new ExamineReplicator(_loggerFactory, luceneIndex, mainDir, tempDir);
            var localLuceneDir = FSDirectory.Open(
                localLuceneIndexFolder,
                LockFactory.GetLockFactory(localLuceneIndexFolder));

            if (forceUnlock)
            {
                IndexWriter.Unlock(localLuceneDir);
            }

            // Start replicating back to main
            _replicator.StartIndexReplicationOnSchedule(1000);

            directory = localLuceneDir;

            return result;
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
            OpenedSuccessfully = 64
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
