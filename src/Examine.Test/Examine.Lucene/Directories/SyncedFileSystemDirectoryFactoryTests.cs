using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Examine.Lucene;
using Examine.Lucene.Analyzers;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Test.Examine.Lucene.Directories
{
    [TestFixture]
    public class SyncedFileSystemDirectoryFactoryTests : ExamineBaseTest
    {
        [TestCase(true, false, SyncedFileSystemDirectoryFactory.CreateResult.NotClean | SyncedFileSystemDirectoryFactory.CreateResult.Fixed | SyncedFileSystemDirectoryFactory.CreateResult.OpenedSuccessfully)]
        [TestCase(true, true, SyncedFileSystemDirectoryFactory.CreateResult.MissingSegments | SyncedFileSystemDirectoryFactory.CreateResult.CorruptCreatedNew, Ignore = "testing")]
        [TestCase(false, false, SyncedFileSystemDirectoryFactory.CreateResult.OpenedSuccessfully, Ignore = "testing")]
        [Test]
        public void Given_ExistingCorruptIndex_When_CreatingDirectory_Then_IndexCreatedOrOpened(
            bool corruptIndex,
            bool removeSegments,
            Enum expected)
        {
            var mainPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                CreateIndex(mainPath, corruptIndex, removeSegments);

                using var syncedDirFactory = new SyncedFileSystemDirectoryFactory(
                    new DirectoryInfo(tempPath),
                    new DirectoryInfo(mainPath),
                    new DefaultLockFactory(),
                    LoggerFactory,
                    true);

                using var index = new LuceneIndex(
                    LoggerFactory,
                    TestIndex.TestIndexName,
                    Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                    {
                        DirectoryFactory = syncedDirFactory,
                    }));

                var result = syncedDirFactory.TryCreateDirectory(index, false, out var dir);

                Assert.IsTrue(result.HasFlag(expected), $"{result} does not have flag {expected}");
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

                using (var syncedDirFactory = new SyncedFileSystemDirectoryFactory(
                    new DirectoryInfo(tempPath),
                    new DirectoryInfo(mainPath),
                    new DefaultLockFactory(),
                    LoggerFactory,
                    true))
                {
                    using var index = new LuceneIndex(
                        LoggerFactory,
                        TestIndex.TestIndexName,
                        Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                        {
                            DirectoryFactory = syncedDirFactory,
                        }));

                    var result = syncedDirFactory.TryCreateDirectory(index, false, out var dir);

                    Assert.IsTrue(result.HasFlag(SyncedFileSystemDirectoryFactory.CreateResult.SyncedFromLocal));
                }   

                // Ensure the docs are there in main
                using var mainIndex = new LuceneIndex(
                    LoggerFactory,
                    TestIndex.TestIndexName,
                    Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                    {
                        DirectoryFactory = new GenericDirectoryFactory(_ => FSDirectory.Open(Path.Combine(mainPath, TestIndex.TestIndexName))),
                    }));

                var searchResults = mainIndex.Searcher.CreateQuery().All().Execute();
                Assert.AreEqual(2, searchResults.TotalItemCount);
            }
            finally
            {
                System.IO.Directory.Delete(mainPath, true);
                System.IO.Directory.Delete(tempPath, true);
            }
        }

        private void CreateIndex(string rootPath, bool corruptIndex, bool removeSegments)
        {
            using var luceneDir = FSDirectory.Open(Path.Combine(rootPath, TestIndex.TestIndexName));

            using (var writer = new IndexWriter(luceneDir, new IndexWriterConfig(LuceneInfo.CurrentVersion, new CultureInvariantStandardAnalyzer())))
            using (var indexer = GetTestIndex(writer))
            using (indexer.WithThreadingMode(IndexThreadingMode.Synchronous))
            {
                indexer.IndexItems(new[]
                {
                        new ValueSet(1.ToString(), "content",
                            new Dictionary<string, IEnumerable<object>>
                            {
                                {"item1", new List<object>(new[] {"value1"})},
                                {"item2", new List<object>(new[] {"value2"})}
                            }),
                        new ValueSet(2.ToString(), "content",
                            new Dictionary<string, IEnumerable<object>>
                            {
                                {"item1", new List<object>(new[] {"value3"})},
                                {"item2", new List<object>(new[] {"value4"})}
                            }),
                    });
            }

            Assert.IsTrue(DirectoryReader.IndexExists(luceneDir));

            if (corruptIndex)
            {
                CorruptIndex(luceneDir.Directory, removeSegments);
            }
        }

        private void CorruptIndex(DirectoryInfo dir, bool removeSegments)
        {
            // Get an index (non segments file) and delete it (corrupt index)
            var indexFile = dir.GetFiles()
                .Where(x => removeSegments
                    ? x.Name.Contains("segments_", StringComparison.OrdinalIgnoreCase)
                    : !x.Name.Contains("segments", StringComparison.OrdinalIgnoreCase))
                .First();

            File.Delete(indexFile.FullName);
        }
    }
}
