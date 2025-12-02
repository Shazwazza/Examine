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
                        index1.IndexItem(
                            new ValueSet(1.ToString(), "content",
                                new Dictionary<string, IEnumerable<object>>
                                {
                                    {"item1", new List<object>(new[] {"value1"})},
                                    {"item2", new List<object>(new[] {"value2"})}
                                }));

                        index2.IndexItem(
                            new ValueSet(1.ToString(), "content",
                                new Dictionary<string, IEnumerable<object>>
                                {
                                    {"item1", new List<object>(new[] {"value1"})},
                                    {"item2", new List<object>(new[] {"value2"})}
                                }));
                    }

                    // Allow time to complete indexing/file writing/syncing
                    Thread.Sleep(1000);

                    host.Dispose();
                }
                finally
                {
                    // Validate that the replication worked and the directory can be read
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
            var mainPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                CreateIndex(mainPath, corruptIndex, removeSegments);

                var syncedDirFactory = new SyncedFileSystemDirectoryFactory(
                    new DirectoryInfo(tempPath),
                    new DirectoryInfo(mainPath),
                    new DefaultLockFactory(),
                    LoggerFactory,
                    Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions()),
                    fixIndex);

                using var index = new LuceneIndex(
                    LoggerFactory,
                    TestIndex.TestIndexName,
                    Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                    {
                        DirectoryFactory = syncedDirFactory
                    }));

                Directory? dir = null;
                try
                {
                    var result = syncedDirFactory.TryCreateDirectory(index, false, out dir);
                    Assert.IsTrue(result.HasFlag(expected), $"{result} does not have flag {expected}");
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

        [Test]
        public void Given_CorruptMainIndex_And_HealthyLocalIndex_When_CreatingDirectory_Then_LocalIndexSyncedToMain()
        {
            var mainPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                // create unhealthy index
                CreateIndex(mainPath, true, false);

                // create healthy index
                CreateIndex(tempPath, false, false);

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

                Directory? dir = null;
                try
                {
                    var result = syncedDirFactory.TryCreateDirectory(index, false, out dir);
                    Assert.IsTrue(result.HasFlag(SyncedFileSystemDirectoryFactory.CreateResult.SyncedFromLocal));
                }
                finally
                {
                    dir?.Dispose();
                }

                // Ensure the docs are there in main
                using var mainIndex = new LuceneIndex(
                    LoggerFactory,
                    TestIndex.TestIndexName,
                    Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                    {
                        DirectoryFactory = new GenericDirectoryFactory(
                            _ => FSDirectory.Open(Path.Combine(mainPath, TestIndex.TestIndexName)),
                            _ => FSDirectory.Open(Path.Combine(mainPath, TestIndex.TestIndexName, "Taxonomy"))),
                    }));

                var searchResults = mainIndex.Searcher.CreateQuery().All().Execute();
                Assert.AreEqual(ItemCount - 2, searchResults.TotalItemCount);
            }
            finally
            {
                System.IO.Directory.Delete(mainPath, true);
                System.IO.Directory.Delete(tempPath, true);
            }
        }

        [Test]
        public void Given_CorruptMainIndex_And_CorruptLocalIndex_When_CreatingDirectory_Then_NewIndexesCreatedAndUsable()
        {
            var mainPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                // create unhealthy index
                CreateIndex(mainPath, true, false);

                // create unhealthy index
                CreateIndex(tempPath, true, false);

                var syncedFactory = new SyncedFileSystemDirectoryFactory(
                    new DirectoryInfo(tempPath),
                    new DirectoryInfo(mainPath),
                    new DefaultLockFactory(),
                    LoggerFactory,
                    Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions()),
                    false);

                // Ensure the docs are there in main
                using var mainIndex = new LuceneIndex(
                    LoggerFactory,
                    TestIndex.TestIndexName,
                    Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                    {
                        DirectoryFactory = syncedFactory,
                    }));

                var searchResults = mainIndex.Searcher.CreateQuery().All().Execute();
                Assert.AreEqual(0, searchResults.TotalItemCount);
            }
            finally
            {
                System.IO.Directory.Delete(mainPath, true);
                System.IO.Directory.Delete(tempPath, true);
            }
        }

        private void CreateIndex(string rootPath, bool corruptIndex, bool removeSegments)
        {
            var logger = LoggerFactory.CreateLogger<SyncedFileSystemDirectoryFactoryTests>();

            var indexPath = Path.Combine(rootPath, TestIndex.TestIndexName);
            logger.LogInformation($"Creating index at {indexPath} with options: corruptIndex: {corruptIndex}, removeSegments: {removeSegments}");

            using var luceneDir = FSDirectory.Open(indexPath);
            using var luceneTaxonomyDir = FSDirectory.Open(Path.Combine(indexPath, "taxonomy"));

            var taxonomyWriterFactory = new SnapshotDirectoryTaxonomyIndexWriterFactory();
            using (var writer = new IndexWriter(luceneDir, new IndexWriterConfig(LuceneInfo.CurrentVersion, new CultureInvariantStandardAnalyzer())))
            using (var taxonomyWriter = new DirectoryTaxonomyWriter(taxonomyWriterFactory, luceneTaxonomyDir))
            using (var indexer = GetTestIndex(writer, taxonomyWriterFactory))
            using (indexer.WithThreadingMode(IndexThreadingMode.Synchronous))
            {
                var valueSets = new List<ValueSet>();
                for (int i = 0; i < ItemCount; i++)
                {
                    valueSets.Add(
                        new ValueSet(i.ToString(), "content",
                            new Dictionary<string, IEnumerable<object>>
                            {
                                {"item1", new List<object>(new[] {"value1"})},
                                {"item2", new List<object>(new[] {"value2"})}
                            }));
                }

                indexer.IndexItems(valueSets);

                // Now delete some items
                indexer.DeleteFromIndex(new[] { "1", "2" });

                // double ensure we commit here
                indexer.IndexWriter.IndexWriter.Commit();
                indexer.IndexWriter.IndexWriter.WaitForMerges();
            }


            logger.LogInformation("Created index at " + luceneDir.Directory);
            Assert.IsTrue(DirectoryReader.IndexExists(luceneDir));

            if (corruptIndex)
            {
                CorruptIndex(luceneDir.Directory, removeSegments, logger);
            }
        }

        private void CorruptIndex(DirectoryInfo dir, bool removeSegments, ILogger logger)
        {
            // index file extensions (no segments, no gen)
            var indexFileExtensions = IndexFileNames.INDEX_EXTENSIONS
                .Except(new[] { IndexFileNames.GEN_EXTENSION })
                .ToArray();

            // Get an index (non segments file) and delete it (corrupt index)
            var indexFile = dir.GetFiles()
                .Where(x => removeSegments
                    ? x.Extension.Contains(Lucene46SegmentInfoFormat.SI_EXTENSION, StringComparison.OrdinalIgnoreCase)
                    : indexFileExtensions.Any(e => IndexFileNames.MatchesExtension(x.Extension, e)))
                .First();

            logger.LogInformation($"Deleting {indexFile.FullName}");
            File.Delete(indexFile.FullName);
        }

        private class MyAppDiscriminator : IApplicationDiscriminator
        {
            public string Discriminator { get; } = Guid.NewGuid().ToString();
        }
    }
}
