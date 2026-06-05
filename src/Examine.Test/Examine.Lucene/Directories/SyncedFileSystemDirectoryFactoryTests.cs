using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Examine.Lucene;
using Examine.Lucene.Analyzers;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Lucene.Net.Codecs.Lucene46;
using Lucene.Net.Facet.Taxonomy.Directory;
using Lucene.Net.Index;
using Lucene.Net.Replicator;
using Lucene.Net.Store;
using Microsoft.AspNetCore.DataProtection.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Test.Examine.Lucene.Directories
{
    [TestFixture]
    [NonParallelizable]
    public class SyncedFileSystemDirectoryFactoryTests : ExamineBaseTest
    {
        private const int ItemCount = 100;

        #region Tests

        [TestCase]
        public void Given_GenericHostBoot_When_Indexed_Then_ReplicationSucceeds()
        {
            var appRoot = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()));
            var applicationDiscriminator = new MyAppDiscriminator();
            var tempDir = new DirectoryInfo(TempEnvFileSystemDirectoryFactory.GetTempPath(
                Mock.Of<IApplicationIdentifier>(x => x.GetApplicationUniqueIdentifier() == applicationDiscriminator.Discriminator)));

            try
            {
                appRoot.Create();

                var builder = Host.CreateApplicationBuilder();
                builder.Logging.AddConsole();
                builder.Logging.SetMinimumLevel(
#if DEBUG
                        LogLevel.Debug
#else
                        LogLevel.Information
#endif
                    );

                var services = builder.Services;

                services.AddSingleton<IApplicationDiscriminator>(applicationDiscriminator);
                services.AddExamine(appRoot);
                services.AddExamineLuceneIndex<LuceneIndex, SyncedFileSystemDirectoryFactory>("MyIndex");
                services.AddExamineLuceneIndex<LuceneIndex, SyncedFileSystemDirectoryFactory>("SyncedIndex");

                var host = builder.Build();

                var manager = host.Services.GetRequiredService<IExamineManager>();
                if (!manager.TryGetIndex("MyIndex", out var i1) || i1 is not LuceneIndex index1)
                {
                    throw new Exception("Index not found");
                }

                if (!manager.TryGetIndex("SyncedIndex", out var i2) || i2 is not LuceneIndex index2)
                {
                    throw new Exception("Index not found");
                }

                var dir1 = (SyncedFileSystemDirectory)index1.GetLuceneDirectory();
                var dir2 = (SyncedFileSystemDirectory)index2.GetLuceneDirectory();

                var dirInfo1 = ((MMapDirectory)((NRTCachingDirectory)dir1.MainLuceneDirectory).Delegate).Directory;
                var dirInfo2 = ((MMapDirectory)((NRTCachingDirectory)dir2.MainLuceneDirectory).Delegate).Directory;
                Assert.IsFalse(dirInfo1.Exists);
                Assert.IsFalse(dirInfo2.Exists);

                try
                {
                    using (index1.WithThreadingMode(IndexThreadingMode.Synchronous))
                    using (index2.WithThreadingMode(IndexThreadingMode.Synchronous))
                    {
                        index1.IndexItem(CreateValueSet(1.ToString()));
                        index2.IndexItem(CreateValueSet(1.ToString()));
                    }

                    Thread.Sleep(1000);
                    host.Dispose();
                }
                finally
                {
                    Assert.IsTrue(dirInfo1.Exists);
                    Assert.IsTrue(dirInfo2.Exists);
                    using var mainDir1 = FSDirectory.Open(dirInfo1);
                    using var mainDir2 = FSDirectory.Open(dirInfo2);
                    Assert.Greater(mainDir1.ListAll().Length, 1);
                    Assert.Greater(mainDir2.ListAll().Length, 1);
                    Assert.IsTrue(DirectoryReader.IndexExists(mainDir1));
                    Assert.IsTrue(DirectoryReader.IndexExists(mainDir2));
                }
            }
            finally
            {
                appRoot.Delete(true);
                tempDir.Delete(true);
            }
        }

        [TestCase(true, false, true, SyncedFileSystemDirectoryFactory.CreateResult.NotClean | SyncedFileSystemDirectoryFactory.CreateResult.Fixed | SyncedFileSystemDirectoryFactory.CreateResult.OpenedSuccessfully)]
        [TestCase(true, false, false, SyncedFileSystemDirectoryFactory.CreateResult.NotClean | SyncedFileSystemDirectoryFactory.CreateResult.CorruptCreatedNew)]
        [TestCase(true, true, false, SyncedFileSystemDirectoryFactory.CreateResult.MissingSegments | SyncedFileSystemDirectoryFactory.CreateResult.CorruptCreatedNew)]
        [TestCase(false, false, false, SyncedFileSystemDirectoryFactory.CreateResult.OpenedSuccessfully)]
        [Test]
        public void Given_ExistingCorruptIndex_When_CreatingDirectory_Then_IndexCreatedOrOpened(
            bool corruptIndex,
            bool removeSegments,
            bool fixIndex,
            Enum expected)
        {
            WithTempPaths((mainPath, tempPath) =>
            {
                CreateIndex(mainPath, corruptIndex, removeSegments);

                var syncedDirFactory = CreateSyncedFactory(tempPath, mainPath, fixIndex: fixIndex);
                using var index = CreateLuceneIndex(syncedDirFactory);

                var result = TryCreateDirectoryWithCleanup(syncedDirFactory, index);
                Assert.IsTrue(result.HasFlag(expected), $"{result} does not have flag {expected}");
            });
        }

        [Test]
        public void Given_CorruptMainIndex_And_HealthyLocalIndex_When_CreatingDirectory_Then_LocalIndexSyncedToMain()
        {
            WithTempPaths((mainPath, tempPath) =>
            {
                CreateIndex(mainPath, corruptIndex: true, removeSegments: false);
                CreateIndex(tempPath, corruptIndex: false, removeSegments: false);

                var syncedDirFactory = CreateSyncedFactory(tempPath, mainPath);
                using var index = CreateLuceneIndex(syncedDirFactory);

                var result = TryCreateDirectoryWithCleanup(syncedDirFactory, index);
                Assert.IsTrue(result.HasFlag(SyncedFileSystemDirectoryFactory.CreateResult.SyncedFromLocal));

                using var mainIndex = CreateMainIndexReader(mainPath);
                var searchResults = mainIndex.Searcher.CreateQuery().All().Execute();
                Assert.AreEqual(ItemCount - 2, searchResults.TotalItemCount);
            });
        }

        [Test]
        public void Given_CorruptMainIndex_And_CorruptLocalIndex_When_CreatingDirectory_Then_NewIndexesCreatedAndUsable()
        {
            WithTempPaths((mainPath, tempPath) =>
            {
                CreateIndex(mainPath, corruptIndex: true, removeSegments: false);
                CreateIndex(tempPath, corruptIndex: true, removeSegments: false);

                var syncedFactory = CreateSyncedFactory(tempPath, mainPath);
                using var mainIndex = CreateLuceneIndex(syncedFactory);

                var searchResults = mainIndex.Searcher.CreateQuery().All().Execute();
                Assert.AreEqual(0, searchResults.TotalItemCount);
            });
        }

        [Test]
        public void Given_NoTaxonomyDirectory_When_CreatingDirectory_Then_IndexCreatedSuccessfully()
        {
            WithTempPaths((mainPath, tempPath) =>
            {
                var syncedDirFactory = CreateSyncedFactory(tempPath, mainPath, useTaxonomy: false);
                using var index = CreateLuceneIndex(syncedDirFactory, useTaxonomy: false);

                var result = TryCreateDirectoryWithCleanup(syncedDirFactory, index);
                Assert.IsTrue(
                    result == SyncedFileSystemDirectoryFactory.CreateResult.Init ||
                    result.HasFlag(SyncedFileSystemDirectoryFactory.CreateResult.OpenedSuccessfully),
                    $"Expected Init or OpenedSuccessfully, got {result}");

                var taxonomyPath = Path.Combine(mainPath, TestIndex.TestIndexName, "taxonomy");
                Assert.IsFalse(System.IO.Directory.Exists(taxonomyPath), "Taxonomy directory should not exist when UseTaxonomyIndex is false");
            });
        }

        [Test]
        public void Given_NoTaxonomyDirectory_When_IndexingData_Then_SearchSucceeds()
        {
            WithTempPaths((mainPath, tempPath) =>
            {
                var syncedFactory = CreateSyncedFactory(tempPath, mainPath, useTaxonomy: false);
                using var index = CreateLuceneIndex(syncedFactory, useTaxonomy: false);

                using (index.WithThreadingMode(IndexThreadingMode.Synchronous))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        index.IndexItem(CreateValueSet(i.ToString(), "value" + i));
                    }
                }

                var searchResults = index.Searcher.CreateQuery().All().Execute();
                Assert.AreEqual(10, searchResults.TotalItemCount);

                var taxonomyPath = Path.Combine(mainPath, TestIndex.TestIndexName, "taxonomy");
                Assert.IsFalse(System.IO.Directory.Exists(taxonomyPath), "Taxonomy directory should not exist when UseTaxonomyIndex is false");
            });
        }

        [Test]
        public void Given_CorruptMainIndex_And_HealthyLocalIndex_NoTaxonomy_When_CreatingDirectory_Then_LocalIndexSyncedToMain()
        {
            WithTempPaths((mainPath, tempPath) =>
            {
                CreateIndexWithoutTaxonomy(mainPath, corruptIndex: true, removeSegments: false);
                CreateIndexWithoutTaxonomy(tempPath, corruptIndex: false, removeSegments: false);

                var syncedDirFactory = CreateSyncedFactory(tempPath, mainPath, useTaxonomy: false);
                using var index = CreateLuceneIndex(syncedDirFactory, useTaxonomy: false);

                var result = TryCreateDirectoryWithCleanup(syncedDirFactory, index);
                Assert.IsTrue(result.HasFlag(SyncedFileSystemDirectoryFactory.CreateResult.SyncedFromLocal));

                using var mainIndex = CreateMainIndexReader(mainPath, useTaxonomy: false);
                var searchResults = mainIndex.Searcher.CreateQuery().All().Execute();
                Assert.AreEqual(ItemCount - 2, searchResults.TotalItemCount);
            });
        }

        [Test]
        public void Given_CorruptMainIndex_And_CorruptLocalIndex_NoTaxonomy_When_CreatingDirectory_Then_NewIndexesCreatedAndUsable()
        {
            WithTempPaths((mainPath, tempPath) =>
            {
                CreateIndexWithoutTaxonomy(mainPath, corruptIndex: true, removeSegments: false);
                CreateIndexWithoutTaxonomy(tempPath, corruptIndex: true, removeSegments: false);

                var syncedFactory = CreateSyncedFactory(tempPath, mainPath, useTaxonomy: false);
                using var mainIndex = CreateLuceneIndex(syncedFactory, useTaxonomy: false);

                var searchResults = mainIndex.Searcher.CreateQuery().All().Execute();
                Assert.AreEqual(0, searchResults.TotalItemCount);
            });
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Creates a temporary test paths structure and executes the test action with automatic cleanup.
        /// </summary>
        private static void WithTempPaths(Action<string, string> testAction)
        {
            var mainPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                testAction(mainPath, tempPath);
            }
            finally
            {
                DeleteDirectoryIfExists(mainPath);
                DeleteDirectoryIfExists(tempPath);
            }
        }

        private static void DeleteDirectoryIfExists(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                System.IO.Directory.Delete(path, true);
            }
        }

        /// <summary>
        /// Creates a mock IOptionsMonitor for LuceneDirectoryIndexOptions.
        /// </summary>
        private static IOptionsMonitor<LuceneDirectoryIndexOptions> CreateDirectoryOptionsMonitor(
            LuceneDirectoryIndexOptions? options = null)
        {
            options ??= new LuceneDirectoryIndexOptions();
            return Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(
                x => x.Get(TestIndex.TestIndexName) == options);
        }

        /// <summary>
        /// Creates a SyncedFileSystemDirectoryFactory with the specified parameters.
        /// </summary>
        private SyncedFileSystemDirectoryFactory CreateSyncedFactory(
            string tempPath,
            string mainPath,
            bool useTaxonomy = true,
            bool fixIndex = false)
        {
            var options = new LuceneDirectoryIndexOptions { UseTaxonomyIndex = useTaxonomy };
            return new SyncedFileSystemDirectoryFactory(
                new DirectoryInfo(tempPath),
                new DirectoryInfo(mainPath),
                new DefaultLockFactory(),
                LoggerFactory,
                CreateDirectoryOptionsMonitor(options),
                fixIndex);
        }

        /// <summary>
        /// Creates a LuceneIndex with the specified directory factory.
        /// </summary>
        private LuceneIndex CreateLuceneIndex(
            IDirectoryFactory directoryFactory,
            bool useTaxonomy = true)
        {
            var options = new LuceneDirectoryIndexOptions
            {
                DirectoryFactory = directoryFactory,
                UseTaxonomyIndex = useTaxonomy
            };
            return new LuceneIndex(
                LoggerFactory,
                TestIndex.TestIndexName,
                CreateDirectoryOptionsMonitor(options));
        }

        /// <summary>
        /// Creates a LuceneIndex using a GenericDirectoryFactory pointing to the main path.
        /// </summary>
        private LuceneIndex CreateMainIndexReader(string mainPath, bool useTaxonomy = true)
        {
            var factory = new GenericDirectoryFactory(
                _ => FSDirectory.Open(Path.Combine(mainPath, TestIndex.TestIndexName)),
                _ => useTaxonomy
                    ? FSDirectory.Open(Path.Combine(mainPath, TestIndex.TestIndexName, "Taxonomy"))
                    : null!);

            return CreateLuceneIndex(factory, useTaxonomy);
        }

        /// <summary>
        /// Creates a standard ValueSet for testing.
        /// </summary>
        private static ValueSet CreateValueSet(string id, string item2Value = "value2")
            => new ValueSet(id, "content",
                new Dictionary<string, IEnumerable<object>>
                {
                    { "item1", new List<object>(new[] { "value1" }) },
                    { "item2", new List<object>(new[] { item2Value }) }
                });

        /// <summary>
        /// Creates a batch of standard ValueSets for testing.
        /// </summary>
        private static List<ValueSet> CreateValueSets(int count)
        {
            var valueSets = new List<ValueSet>(count);
            for (var i = 0; i < count; i++)
            {
                valueSets.Add(CreateValueSet(i.ToString()));
            }
            return valueSets;
        }

        /// <summary>
        /// Executes the factory's TryCreateDirectory and ensures cleanup.
        /// </summary>
        private static SyncedFileSystemDirectoryFactory.CreateResult TryCreateDirectoryWithCleanup(
            SyncedFileSystemDirectoryFactory factory,
            LuceneIndex index)
        {
            Directory? dir = null;
            try
            {
                return factory.TryCreateDirectory(index, false, out dir);
            }
            finally
            {
                dir?.Dispose();
            }
        }

        /// <summary>
        /// Regression test for https://github.com/Shazwazza/Examine/issues/434.
        /// When a single file in the local temp folder is locked while syncing from main storage,
        /// directory creation fails - but once the lock is released a subsequent attempt must recover
        /// (previously the exception was permanently cached by the index's Lazy directory).
        /// File locking semantics differ across platforms, so this reproduction is Windows-only.
        /// </summary>
        [Test]
        [Platform(Include = "Win")]
        public void Given_LockedFileInLocalTemp_When_CreatingDirectory_Then_RecoversAfterLockReleased()
        {
            var mainPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                // healthy main index
                CreateIndex(mainPath, false, false);

                // place a locked file in the local temp index folder that ClearDirectory will try to delete
                var localIndexFolder = new DirectoryInfo(Path.Combine(tempPath, TestIndex.TestIndexName));
                localIndexFolder.Create();
                var lockedFilePath = Path.Combine(localIndexFolder.FullName, "locked.tmp");
                File.WriteAllText(lockedFilePath, "locked");

                var syncedDirFactory = new SyncedFileSystemDirectoryFactory(
                    new DirectoryInfo(tempPath),
                    new DirectoryInfo(mainPath),
                    new DefaultLockFactory(),
                    LoggerFactory,
                    Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions()),
                    false);

                using var index = new LuceneIndex(
                    LoggerFactory,
                    TestIndex.TestIndexName,
                    Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                    {
                        DirectoryFactory = syncedDirFactory
                    }));

                using (var lockStream = new FileStream(lockedFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // the locked file cannot be deleted, so directory creation fails
                    Assert.Throws<IOException>(() =>
                    {
                        syncedDirFactory.TryCreateDirectory(index, false, out _);
                    });
                }

                // lock released - a subsequent attempt must recover
                Directory? dir = null;
                try
                {
                    Assert.DoesNotThrow(() =>
                    {
                        syncedDirFactory.TryCreateDirectory(index, false, out dir);
                    });
                }
                finally
                {
                    dir?.Dispose();
                }
            }
            finally
            {
                System.IO.Directory.Delete(mainPath, true);
                System.IO.Directory.Delete(tempPath, true);
            }
        }

        private void CreateIndex(string rootPath, bool corruptIndex, bool removeSegments)
            => CreateIndex(rootPath, corruptIndex, removeSegments, useTaxonomy: true);

        private void CreateIndexWithoutTaxonomy(string rootPath, bool corruptIndex, bool removeSegments)
            => CreateIndex(rootPath, corruptIndex, removeSegments, useTaxonomy: false);

        private void CreateIndex(string rootPath, bool corruptIndex, bool removeSegments, bool useTaxonomy)
        {
            var logger = LoggerFactory.CreateLogger<SyncedFileSystemDirectoryFactoryTests>();
            var indexPath = Path.Combine(rootPath, TestIndex.TestIndexName);
            logger.LogInformation($"Creating index at {indexPath} with options: corruptIndex: {corruptIndex}, removeSegments: {removeSegments}, useTaxonomy: {useTaxonomy}");

            using var luceneDir = FSDirectory.Open(indexPath);

            if (useTaxonomy)
            {
                using var luceneTaxonomyDir = FSDirectory.Open(Path.Combine(indexPath, "taxonomy"));
                var taxonomyWriterFactory = new SnapshotDirectoryTaxonomyIndexWriterFactory();
                using var writer = new IndexWriter(luceneDir, new IndexWriterConfig(LuceneInfo.CurrentVersion, new CultureInvariantStandardAnalyzer()));
                using var taxonomyWriter = new DirectoryTaxonomyWriter(taxonomyWriterFactory, luceneTaxonomyDir);
                using var indexer = GetTestIndex(writer, taxonomyWriterFactory);
                PopulateIndex(indexer);
            }
            else
            {
                using var writer = new IndexWriter(luceneDir, new IndexWriterConfig(LuceneInfo.CurrentVersion, new CultureInvariantStandardAnalyzer()));
                using var indexer = GetTestIndexWithoutTaxonomy(writer);
                PopulateIndex(indexer);
            }

            logger.LogInformation("Created index at " + luceneDir.Directory);
            Assert.IsTrue(DirectoryReader.IndexExists(luceneDir));

            if (corruptIndex)
            {
                CorruptIndex(luceneDir.Directory, removeSegments, logger);
            }
        }

        private static void PopulateIndex(TestIndex indexer)
        {
            using (indexer.WithThreadingMode(IndexThreadingMode.Synchronous))
            {
                indexer.IndexItems(CreateValueSets(ItemCount));
                indexer.DeleteFromIndex(new[] { "1", "2" });
                indexer.IndexWriter.IndexWriter.Commit();
                indexer.IndexWriter.IndexWriter.WaitForMerges();
            }
        }

        private static void CorruptIndex(DirectoryInfo dir, bool removeSegments, ILogger logger)
        {
            var indexFileExtensions = IndexFileNames.INDEX_EXTENSIONS
                .Except(new[] { IndexFileNames.GEN_EXTENSION })
                .ToArray();

            var indexFile = dir.GetFiles()
                .Where(x => removeSegments
                    ? x.Extension.Contains(Lucene46SegmentInfoFormat.SI_EXTENSION, StringComparison.OrdinalIgnoreCase)
                    : indexFileExtensions.Any(e => IndexFileNames.MatchesExtension(x.Extension, e)))
                .First();

            logger.LogInformation($"Deleting {indexFile.FullName}");
            File.Delete(indexFile.FullName);
        }

        private TestIndex GetTestIndexWithoutTaxonomy(IndexWriter writer)
            => new TestIndex(
                LoggerFactory,
                Mock.Of<IOptionsMonitor<LuceneIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneIndexOptions
                {
                    UseTaxonomyIndex = false
                }),
                writer,
                null!);

        #endregion

        private class MyAppDiscriminator : IApplicationDiscriminator
        {
            public string Discriminator { get; } = Guid.NewGuid().ToString();
        }
    }
}
