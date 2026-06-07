using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Examine.Lucene;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Replicator;
using Lucene.Net.Store;
using StoreLock = Lucene.Net.Store.Lock;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Sync
{
    [TestFixture]
    public class ExamineReplicatorTests : ExamineBaseTest
    {
        private readonly ILoggerFactory _loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(
            x => x.AddConsole()
                .SetMinimumLevel(
#if DEBUG
                    LogLevel.Debug
#else
                    LogLevel.Information
#endif
                ));

        private readonly ILogger<ExamineReplicator> _replicatorLogger;
        private readonly ILogger<LoggingReplicationClient> _clientLogger;

        public ExamineReplicatorTests()
        {
            _replicatorLogger = _loggerFactory.CreateLogger<ExamineReplicator>();
            _clientLogger = _loggerFactory.CreateLogger<LoggingReplicationClient>();
        }

        [Test]
        public void GivenAMainIndex_WhenReplicatedLocally_TheLocalIndexIsPopulated()
        {
            var tempStorage = new System.IO.DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            var indexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());

            using (var mainDir = new RandomIdRAMDirectory())
            using (var mainTaxonomyDir = new RandomIdRAMDirectory())
            using (var localDir = new RandomIdRAMDirectory())
            using (var localTaxonomyDir = new RandomIdRAMDirectory())
            using (var mainIndex = GetTestIndex(mainDir, mainTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion), indexDeletionPolicy: indexDeletionPolicy))
            using (var replicator = new ExamineReplicator(_replicatorLogger, _clientLogger, mainIndex, mainDir, localDir, localTaxonomyDir, tempStorage))
            {
                mainIndex.CreateIndex();

                mainIndex.IndexItems(TestIndex.AllData());

                var mainReader = mainIndex.IndexWriter.IndexWriter.GetReader(true);
                Assert.AreEqual(100, mainReader.NumDocs);

                // TODO: Ok so replication CANNOT occur on an open index with an open IndexWriter.
                // See this note: https://lucenenet.apache.org/docs/4.8.0-beta00014/api/replicator/Lucene.Net.Replicator.IndexReplicationHandler.html
                // "NOTE: This handler assumes that Lucene.Net.Index.IndexWriter is not opened by another process on the index directory. In fact, opening an Lucene.Net.Index.IndexWriter on the same directory to which files are copied can lead to undefined behavior, where some or all the files will be deleted, override other files or simply create a mess. When you replicate an index, it is best if the index is never modified by Lucene.Net.Index.IndexWriter, except the one that is open on the source index, from which you replicate."
                // So if we want to replicate, we can sync from Main on startup and ensure that the writer isn't opened until that
                // is done (the callback can be used for that).
                // If we want to sync back to main, it means we can never open a writer to main, but that might be ok and we
                // publish on a schedule.
                replicator.ReplicateIndex();

                using (var localIndex = GetTestIndex(localDir, localTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
                {
                    var localReader = localIndex.IndexWriter.IndexWriter.GetReader(true);
                    Assert.AreEqual(100, localReader.NumDocs);
                }
            }
        }

        [Test]
        public void GivenAnOpenedWriter_WhenReplicationAttempted_ThenAnExceptionIsThrown()
        {
            var tempStorage = new System.IO.DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            var indexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());

            using (var mainDir = new RandomIdRAMDirectory())
            using (var mainTaxonomyDir = new RandomIdRAMDirectory())
            using (var localDir = new RandomIdRAMDirectory())
            using (var localTaxonomyDir = new RandomIdRAMDirectory())
            using (var mainIndex = GetTestIndex(mainDir, mainTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion), indexDeletionPolicy: indexDeletionPolicy))
            using (var replicator = new ExamineReplicator(_replicatorLogger, _clientLogger, mainIndex, mainDir, localDir, localTaxonomyDir, tempStorage))
            using (var localIndex = GetTestIndex(localDir, localTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
            {
                mainIndex.CreateIndex();

                // this will open the writer
                localIndex.IndexItem(new ValueSet(9999.ToString(), "content",
                            new Dictionary<string, IEnumerable<object>>
                            {
                                {"item1", new List<object>(new[] {"value1"})},
                                {"item2", new List<object>(new[] {"value2"})}
                            }));

                mainIndex.IndexItems(TestIndex.AllData());

                Assert.Throws<InvalidOperationException>(() => replicator.ReplicateIndex());
            }
        }

        [Test]
        public void GivenALockedDestination_WhenStartingScheduledReplication_ThenNoExceptionIsThrown()
        {
            var tempStorage = new System.IO.DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            var indexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());

            using (var mainDir = new RandomIdRAMDirectory())
            using (var mainTaxonomyDir = new RandomIdRAMDirectory())
            using (var localDir = new RandomIdRAMDirectory())
            using (var localTaxonomyDir = new RandomIdRAMDirectory())
            using (var mainIndex = GetTestIndex(mainDir, mainTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion), indexDeletionPolicy: indexDeletionPolicy))
            using (var replicator = new ExamineReplicator(_replicatorLogger, _clientLogger, mainIndex, mainDir, localDir, null, tempStorage))
            {
                mainIndex.CreateIndex();

                using (var localIndex = GetTestIndex(localDir, localTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
                {
                    // Open and keep destination writer active so the replicator cannot start.
                    localIndex.IndexItem(new ValueSet(9999.ToString(), "content",
                        new Dictionary<string, IEnumerable<object>>
                        {
                            {"item1", new List<object>(new[] {"value1"})},
                            {"item2", new List<object>(new[] {"value2"})}
                        }));

                    Assert.IsTrue(IndexWriter.IsLocked(localDir));
                    Assert.DoesNotThrow(() => replicator.StartIndexReplicationOnSchedule(1000));
                }

                var startedField = typeof(ExamineReplicator).GetField("_started", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.That(startedField, Is.Not.Null);
                Assert.IsFalse((bool?)startedField!.GetValue(replicator) ?? true);

                // Lock has been released, so retrying should now start replication.
                Assert.DoesNotThrow(() => replicator.StartIndexReplicationOnSchedule(1000));
                Assert.IsTrue((bool?)startedField!.GetValue(replicator) ?? false);
            }
        }

        [Test]
        public void GivenRepeatedReplicationFailures_WhenThresholdReached_ThenReplicationIsUnhealthyAndStopped()
        {
            var tempStorage = new System.IO.DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            var indexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());

            using (var mainDir = new RandomIdRAMDirectory())
            using (var mainTaxonomyDir = new RandomIdRAMDirectory())
            using (var localDir = new RandomIdRAMDirectory())
            using (var mainIndex = GetTestIndex(mainDir, mainTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion), indexDeletionPolicy: indexDeletionPolicy))
            {
                var replicator = new ExamineReplicator(_replicatorLogger, _clientLogger, mainIndex, mainDir, localDir, null, tempStorage);

                mainIndex.CreateIndex();
                mainIndex.IndexItems(TestIndex.AllData());

                replicator.MaxConsecutiveReplicationFailures = 2;
                Assert.IsTrue(replicator.IsReplicationHealthy);
                Assert.AreEqual(0, replicator.ConsecutiveReplicationFailures);

                // Dispose the replicator so the underlying LocalReplicator is closed and every
                // subsequent publish attempt fails, simulating a persistent replication failure.
                replicator.Dispose();

                var commitHandler = typeof(ExamineReplicator).GetMethod("SourceIndex_IndexCommitted", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.That(commitHandler, Is.Not.Null);

                // First failure: still considered healthy (below threshold).
                commitHandler!.Invoke(replicator, new object?[] { mainIndex, EventArgs.Empty });
                Assert.AreEqual(1, replicator.ConsecutiveReplicationFailures);
                Assert.IsTrue(replicator.IsReplicationHealthy);

                // Second failure: threshold reached, replication stops and is reported as unhealthy.
                commitHandler!.Invoke(replicator, new object?[] { mainIndex, EventArgs.Empty });
                Assert.AreEqual(2, replicator.ConsecutiveReplicationFailures);
                Assert.IsFalse(replicator.IsReplicationHealthy);
            }
        }

        [Test]
        public void GivenReplicationStoppedAfterThreshold_WhenRestarted_ThenReplicationRecoversAndIsHealthy()
        {
            var tempStorage = new System.IO.DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            var indexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());

            using (var mainDir = new RandomIdRAMDirectory())
            using (var mainTaxonomyDir = new RandomIdRAMDirectory())
            using (var localDir = new RandomIdRAMDirectory())
            using (var mainIndex = GetTestIndex(mainDir, mainTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion), indexDeletionPolicy: indexDeletionPolicy))
            {
                using (var replicator = new ExamineReplicator(_replicatorLogger, _clientLogger, mainIndex, mainDir, localDir, null, tempStorage))
                {
                    mainIndex.CreateIndex();
                    mainIndex.IndexItems(TestIndex.AllData());

                    replicator.MaxConsecutiveReplicationFailures = 1;

                    // Use a long interval so the background update thread does not interfere; failures are
                    // driven deterministically by invoking the commit handler directly.
                    replicator.StartIndexReplicationOnSchedule(60000);

                    var startedField = typeof(ExamineReplicator).GetField("_started", BindingFlags.NonPublic | BindingFlags.Instance);
                    Assert.That(startedField, Is.Not.Null);
                    Assert.IsTrue((bool)startedField!.GetValue(replicator)!);

                    // Dispose only the underlying LocalReplicator so every publish fails while the replication
                    // client itself stays open, simulating a persistent (non-transient) replication failure.
                    var replicatorField = typeof(ExamineReplicator).GetField("_replicator", BindingFlags.NonPublic | BindingFlags.Instance);
                    Assert.That(replicatorField, Is.Not.Null);
                    var localReplicator = (IDisposable)replicatorField!.GetValue(replicator)!;
                    localReplicator.Dispose();

                    var commitHandler = typeof(ExamineReplicator).GetMethod("SourceIndex_IndexCommitted", BindingFlags.NonPublic | BindingFlags.Instance);
                    Assert.That(commitHandler, Is.Not.Null);

                    // Reaching the threshold (1) stops replication: unhealthy, the update thread is stopped and
                    // _started is reset so a restart is possible.
                    commitHandler!.Invoke(replicator, new object?[] { mainIndex, EventArgs.Empty });
                    Assert.IsFalse(replicator.IsReplicationHealthy);
                    Assert.IsFalse((bool)startedField.GetValue(replicator)!);

                    // Restarting must not throw (the previous update thread was stopped) and replication recovers:
                    // the failure counter is reset and the schedule is running again.
                    Assert.DoesNotThrow(() => replicator.StartIndexReplicationOnSchedule(60000));
                    Assert.IsTrue((bool)startedField.GetValue(replicator)!);
                    Assert.AreEqual(0, replicator.ConsecutiveReplicationFailures);
                    Assert.IsTrue(replicator.IsReplicationHealthy);
                }
            }
        }

        [Test]
        public void GivenASyncedLocalIndex_WhenTriggered_ThenSyncedBackToMainIndex()
        {
            var tempStorage = new System.IO.DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            var indexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());

            using (var mainDir = new RandomIdRAMDirectory())
            using (var mainTaxonomyDir = new RandomIdRAMDirectory())
            using (var localDir = new RandomIdRAMDirectory())
            using (var localTaxonomyDir = new RandomIdRAMDirectory())
            {
                using (var mainIndex = GetTestIndex(mainDir, mainTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion), indexDeletionPolicy: indexDeletionPolicy))
                using (var replicator = new ExamineReplicator(_replicatorLogger, _clientLogger, mainIndex, mainDir, localDir, localTaxonomyDir, tempStorage))
                {
                    mainIndex.CreateIndex();
                    mainIndex.IndexItems(TestIndex.AllData());
                    replicator.ReplicateIndex();
                }

                using (var localIndex = GetTestIndex(localDir, localTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion), indexDeletionPolicy: indexDeletionPolicy))
                {
                    localIndex.IndexItem(new ValueSet(9999.ToString(), "content",
                            new Dictionary<string, IEnumerable<object>>
                            {
                                {"item1", new List<object>(new[] {"value1"})},
                                {"item2", new List<object>(new[] {"value2"})}
                            }));

                    using (var replicator = new ExamineReplicator(_replicatorLogger, _clientLogger, localIndex, localDir, mainDir, mainTaxonomyDir, tempStorage))
                    {
                        // replicate back to main, main index must be closed
                        replicator.ReplicateIndex();
                    }

                    using (var mainIndex = GetTestIndex(mainDir, mainTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
                    {
                        var mainReader = mainIndex.IndexWriter.IndexWriter.GetReader(true);
                        Assert.AreEqual(101, mainReader.NumDocs);
                    }
                }
            }

        }

        [Test]
        public void GivenASyncedLocalIndex_ThenSyncedBackToMainIndexOnSchedule()
        {
            var tempStorage = new System.IO.DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            var indexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());

            using (var mainDir = new RandomIdRAMDirectory())
            using (var mainTaxonomyDir = new RandomIdRAMDirectory())
            using (var localDir = new RandomIdRAMDirectory())
            using (var localTaxonomyDir = new RandomIdRAMDirectory())
            {
                using (var mainIndex = GetTestIndex(mainDir, mainTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion), indexDeletionPolicy: indexDeletionPolicy))
                using (var replicator = new ExamineReplicator(_replicatorLogger, _clientLogger, mainIndex, mainDir, localDir, localTaxonomyDir, tempStorage))
                {
                    mainIndex.CreateIndex();
                    mainIndex.IndexItems(TestIndex.AllData());
                    replicator.ReplicateIndex();
                }

                using (var localIndex = GetTestIndex(localDir, localTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion), indexDeletionPolicy: indexDeletionPolicy))
                {
                    using (var replicator = new ExamineReplicator(_replicatorLogger, _clientLogger, localIndex, localDir, mainDir, mainTaxonomyDir, tempStorage))
                    {
                        // replicate back to main on schedule
                        replicator.StartIndexReplicationOnSchedule(1000);

                        for (var i = 0; i < 10; i++)
                        {
                            localIndex.IndexItem(new ValueSet(("testing" + i).ToString(), "content",
                            new Dictionary<string, IEnumerable<object>>
                            {
                                {"item1", new List<object>(new[] {"value1"})},
                                {"item2", new List<object>(new[] {"value2"})}
                            }));

                            Thread.Sleep(500);
                        }

                        // should be plenty to resync everything
                        Thread.Sleep(2000);
                    }

                    using (var mainIndex = GetTestIndex(mainDir, mainTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
                    {
                        var mainReader = mainIndex.IndexWriter.IndexWriter.GetReader(true);
                        Assert.AreEqual(110, mainReader.NumDocs);
                    }
                }
            }

        }

        [Test]
        public void GivenTaxonomyEnabledIndexWithoutInitializedTaxonomyWriter_WhenCreatingRevision_ThenNoExceptionThrown()
        {
            var tempStorage = new System.IO.DirectoryInfo(TestContext.CurrentContext.WorkDirectory);

            using (var mainDir = new RandomIdRAMDirectory())
            using (var mainTaxonomyDir = new RandomIdRAMDirectory())
            using (var localDir = new RandomIdRAMDirectory())
            using (var localTaxonomyDir = new RandomIdRAMDirectory())
            using (var mainIndex = GetTestIndex(mainDir, mainTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
            using (var replicator = new ExamineReplicator(_replicatorLogger, _clientLogger, mainIndex, mainDir, localDir, localTaxonomyDir, tempStorage))
            {
                mainIndex.CreateIndex();
                Assert.DoesNotThrow(() => replicator.ReplicateIndex());
            }
        }

        /// <summary>
        /// Regression test for https://github.com/Shazwazza/Examine/issues/452.
        /// When the taxonomy directory's write lock is transiently unavailable while the taxonomy writer
        /// is first initialized (e.g. during the brief lock hand-off after the index is synced from main
        /// storage, which is observed on Windows/Azure), the taxonomy writer creation must recover by
        /// retrying instead of permanently returning <c>null</c>. Previously a single transient failure
        /// nulled the taxonomy writer, which made <see cref="ExamineReplicator.CreateRevision"/> throw
        /// "Taxonomy replication is enabled but the taxonomy writer could not be initialized." and the
        /// revision was never published, so changes were never replicated to main storage.
        /// </summary>
        [Test]
        public void GivenTransientTaxonomyLock_WhenInitializingTaxonomyWriter_ThenItRecoversInsteadOfReturningNull()
        {
            using var mainDir = new RandomIdRAMDirectory();
            using var innerTaxonomyDir = new RandomIdRAMDirectory();

            // First, create a healthy, committed search + taxonomy index in the directories so that the
            // second index opens an existing index (and therefore goes straight to lazily creating the
            // taxonomy writer, which is the path that previously failed on a transient lock).
            using (var seedIndex = GetTestIndex(mainDir, innerTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion)))
            {
                seedIndex.CreateIndex();
            }

            // Wrap the taxonomy directory so the first write-lock check fails transiently.
            using var transientTaxonomyDir = new TransientWriteLockDirectory(innerTaxonomyDir, failTimes: 1);

            using var index = GetTestIndex(mainDir, transientTaxonomyDir, new StandardAnalyzer(LuceneInfo.CurrentVersion));

            // Accessing the taxonomy writer triggers CreateTaxonomyWriterWithLockCheck, which hits the
            // transient lock failure. With the fix this retries and succeeds; previously it returned null.
            var taxonomyWriter = index.TaxonomyWriter;

            Assert.IsNotNull(taxonomyWriter, "The taxonomy writer should recover from a transient lock instead of returning null.");
            Assert.IsNotNull(
                index.SnapshotDirectoryTaxonomyIndexWriterFactory?.IndexWriter,
                "The snapshot taxonomy factory's IndexWriter must be initialized so a taxonomy revision can be created.");
            Assert.GreaterOrEqual(
                transientTaxonomyDir.IsLockedCallCount,
                2,
                "The lock check should have been retried after the first transient failure.");
        }

        /// <summary>
        /// A <see cref="FilterDirectory"/> that simulates a transient failure when the index write lock is
        /// checked. The first <paramref name="failTimes"/> calls to check the write lock throw an
        /// <see cref="System.IO.IOException"/>; subsequent calls delegate to the wrapped directory. This
        /// mimics the momentary lock contention observed on Windows/Azure during the local index hand-off.
        /// </summary>
        private sealed class TransientWriteLockDirectory : FilterDirectory
        {
            private readonly int _failTimes;
            private int _isLockedCallCount;

            public TransientWriteLockDirectory(global::Lucene.Net.Store.Directory directory, int failTimes)
                : base(directory)
                => _failTimes = failTimes;

            public int IsLockedCallCount => Volatile.Read(ref _isLockedCallCount);

            public override StoreLock MakeLock(string name) => new TransientLock(base.MakeLock(name), this, name);

            private bool ShouldFailLockCheck(string name)
                => string.Equals(name, IndexWriter.WRITE_LOCK_NAME, StringComparison.Ordinal)
                   && Interlocked.Increment(ref _isLockedCallCount) <= _failTimes;

            private sealed class TransientLock : StoreLock
            {
                private readonly StoreLock _inner;
                private readonly TransientWriteLockDirectory _owner;
                private readonly string _name;

                public TransientLock(StoreLock inner, TransientWriteLockDirectory owner, string name)
                {
                    _inner = inner;
                    _owner = owner;
                    _name = name;
                }

                public override bool Obtain() => _inner.Obtain();

                public override bool IsLocked()
                {
                    if (_owner.ShouldFailLockCheck(_name))
                    {
                        throw new System.IO.IOException("Simulated transient write lock check failure");
                    }

                    return _inner.IsLocked();
                }

                protected override void Dispose(bool disposing)
                {
                    if (disposing)
                    {
                        _inner.Dispose();
                    }
                }
            }
        }
    }
}
