using System;
using System.IO;
using System.Threading;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Test.Examine.Lucene
{
    [TestFixture]
    public class LuceneIndexDirectoryRecoveryTests : ExamineBaseTest
    {
        /// <summary>
        /// Regression test for https://github.com/Shazwazza/Examine/issues/434.
        /// A transient failure when creating the Lucene directory (e.g. a momentarily locked index
        /// file during an Azure App Service / Umbraco Cloud overlapped recycle) must not permanently
        /// poison the index. Previously the directory was created via a <see cref="Lazy{T}"/> which
        /// cached the exception and re-threw it on every subsequent request until the process restarted.
        /// </summary>
        [Test]
        public void Given_TransientDirectoryCreationFailure_When_AccessedAgain_Then_Recovers()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                var failingFactory = new TransientFailingDirectoryFactory(path);

                using var index = new LuceneIndex(
                    LoggerFactory,
                    TestIndex.TestIndexName,
                    Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                    {
                        DirectoryFactory = failingFactory
                    }));

                // First access should surface the transient failure.
                Assert.Throws<InvalidOperationException>(() => index.IndexExists());

                // The failure must not have been cached - a subsequent access recovers and
                // successfully reads the (empty) directory.
                Assert.DoesNotThrow(() => index.IndexExists());
                Assert.AreEqual(2, failingFactory.CreateCount);
            }
            finally
            {
                if (System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.Delete(path, true);
                }
            }
        }

        private class TransientFailingDirectoryFactory : DirectoryFactoryBase
        {
            private readonly string _path;
            private int _createCount;

            public TransientFailingDirectoryFactory(string path) => _path = path;

            public int CreateCount => Volatile.Read(ref _createCount);

            protected override Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
            {
                var count = Interlocked.Increment(ref _createCount);
                if (count == 1)
                {
                    // Simulate a transient failure on the first creation attempt.
                    throw new InvalidOperationException("Simulated transient directory creation failure");
                }

                var dir = FSDirectory.Open(new DirectoryInfo(Path.Combine(_path, luceneIndex.Name)));
                if (forceUnlock)
                {
                    IndexWriter.Unlock(dir);
                }

                return dir;
            }
        }
    }
}
