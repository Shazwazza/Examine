using System;
using System.Collections.Generic;
using System.IO;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Lucene.Net.Store;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Index
{
    /// <summary>
    /// Regression tests for https://github.com/Shazwazza/Examine/issues/629.
    /// When an index folder is deleted from disk while the application is running (e.g. by an OS temp
    /// folder cleaner), an explicit <see cref="LuceneIndex.CreateIndex"/> must restore a usable, empty index.
    /// </summary>
    [TestFixture]
    [NonParallelizable]
    public class LuceneIndexDeletedDirectoryTests : ExamineBaseTest
    {
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void Given_IndexFolderDeleted_When_CreateIndex_Then_IndexIsUsableAgain(bool nrtEnabled, bool useTaxonomy)
            => WithTempPaths((mainPath, _) =>
            {
                var options = new LuceneDirectoryIndexOptions
                {
                    NrtEnabled = nrtEnabled,
                    UseTaxonomyIndex = useTaxonomy
                };
                var optionsMonitor = CreateOptionsMonitor(options);
                options.DirectoryFactory = new FileSystemDirectoryFactory(new DirectoryInfo(mainPath), new SimpleFsLockFactory(), optionsMonitor);

                using var index = new LuceneIndex(LoggerFactory, TestIndex.TestIndexName, optionsMonitor);

                AssertRecoversAfterFolderDeleted(index, Path.Combine(mainPath, TestIndex.TestIndexName));
            });

        [TestCase(true)]
        [TestCase(false)]
        public void Given_SyncedLocalIndexFolderDeleted_When_CreateIndex_Then_IndexIsUsableAgain(bool useTaxonomy)
            => WithTempPaths((mainPath, localPath) =>
            {
                var options = new LuceneDirectoryIndexOptions { UseTaxonomyIndex = useTaxonomy };
                var optionsMonitor = CreateOptionsMonitor(options);
                options.DirectoryFactory = new SyncedFileSystemDirectoryFactory(
                    new DirectoryInfo(localPath),
                    new DirectoryInfo(mainPath),
                    new SimpleFsLockFactory(),
                    LoggerFactory,
                    optionsMonitor);

                using var index = new LuceneIndex(LoggerFactory, TestIndex.TestIndexName, optionsMonitor);

                AssertRecoversAfterFolderDeleted(index, Path.Combine(localPath, TestIndex.TestIndexName));
            });

        private static void AssertRecoversAfterFolderDeleted(LuceneIndex index, string indexPath)
        {
            using (index.WithThreadingMode(IndexThreadingMode.Synchronous))
            {
                index.IndexItems(new[] { CreateValueSet("1"), CreateValueSet("2") });
            }

            // Simulate an external cleaner removing the index folder before the first search.
            Assert.IsTrue(System.IO.Directory.Exists(indexPath));
            System.IO.Directory.Delete(indexPath, true);

            index.CreateIndex();

            Assert.IsTrue(System.IO.Directory.Exists(indexPath), "CreateIndex should recreate the deleted index folder.");
            Assert.AreEqual(0, index.Searcher.CreateQuery().All().Execute().TotalItemCount);

            using (index.WithThreadingMode(IndexThreadingMode.Synchronous))
            {
                index.IndexItems(new[] { CreateValueSet("3") });
            }

            var results = index.Searcher.CreateQuery().All().Execute();
            Assert.AreEqual(1, results.TotalItemCount);
        }

        /// <summary>
        /// Uses marker-file locks, as Umbraco does, so nothing holds the folder open and it can be deleted
        /// externally (the default native locks keep write.lock open on Windows).
        /// </summary>
        private class SimpleFsLockFactory : ILockFactory
        {
            public LockFactory GetLockFactory(DirectoryInfo directory) => new SimpleFSLockFactory(directory);
        }

        private static IOptionsMonitor<LuceneDirectoryIndexOptions> CreateOptionsMonitor(LuceneDirectoryIndexOptions options)
            => Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == options);

        private static ValueSet CreateValueSet(string id)
            => new ValueSet(id, "content", new Dictionary<string, IEnumerable<object>>
            {
                { "item1", new List<object> { "value" + id } }
            });

        private static void WithTempPaths(Action<string, string> testAction)
        {
            var mainPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var localPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                testAction(mainPath, localPath);
            }
            finally
            {
                DeleteDirectoryIfExists(mainPath);
                DeleteDirectoryIfExists(localPath);
            }
        }

        private static void DeleteDirectoryIfExists(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                System.IO.Directory.Delete(path, true);
            }
        }
    }
}
