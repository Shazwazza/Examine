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

namespace Examine.Test.Examine.Lucene.Directories
{
    [TestFixture]
    public class SyncedFileSystemDirectoryFactoryTests : ExamineBaseTest
    {
        [Test]
        public void Given_ExistingCorruptIndex_When_CreatingDirectory_Then_IndexFixed()
        {
            var mainPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                using (var mainDir = FSDirectory.Open(Path.Combine(mainPath, TestIndex.TestIndexName)))
                {
                    using (var writer = new IndexWriter(mainDir, new IndexWriterConfig(LuceneInfo.CurrentVersion, new CultureInvariantStandardAnalyzer())))
                    using (var indexer = GetTestIndex(writer))
                    {
                        using (indexer.WithThreadingMode(IndexThreadingMode.Synchronous))
                        {
                            indexer.IndexItem(new ValueSet(1.ToString(), "content",
                                new Dictionary<string, IEnumerable<object>>
                                {
                                    {"item1", new List<object>(new[] {"value1"})},
                                    {"item2", new List<object>(new[] {"value2"})}
                                }));
                        }
                    }

                    Assert.IsTrue(DirectoryReader.IndexExists(mainDir));

                    // Get an index (non segments file) and delete it (corrupt index)
                    var indexFile = mainDir.Directory.GetFiles().Where(x => !x.Name.Contains("segment", StringComparison.OrdinalIgnoreCase)).First();
                    File.Delete(indexFile.FullName);
                }

                using var syncedDirFactory = new SyncedFileSystemDirectoryFactory(
                    new DirectoryInfo(tempPath),
                    new DirectoryInfo(mainPath),
                    new DefaultLockFactory(),
                    LoggerFactory);

                using var index = new LuceneIndex(
                    LoggerFactory,
                    TestIndex.TestIndexName,
                    Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                    {
                        DirectoryFactory = syncedDirFactory,
                    }));

                var result = syncedDirFactory.TryCreateDirectory(index, false, out var dir);

                Assert.IsTrue(result.HasFlag(SyncedFileSystemDirectoryFactory.CreateResult.NotClean));
                Assert.IsTrue(result.HasFlag(SyncedFileSystemDirectoryFactory.CreateResult.Fixed));
                Assert.IsTrue(result.HasFlag(SyncedFileSystemDirectoryFactory.CreateResult.OpenedSuccessfully));
            }
            finally
            {
                System.IO.Directory.Delete(mainPath, true);
                System.IO.Directory.Delete(tempPath, true);
            }
        }
    }
}
