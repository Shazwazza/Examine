using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lucene.Net.Index;
using static Lucene.Net.Index.IndexWriter;

namespace Examine.LuceneEngine.Directories
{
    public abstract class ExamineDirectory : Lucene.Net.Store.Directory
    {
        public ExamineDirectory()
        {
            SyncOnCommit = true;
            _onCommitAction = (indexWriter) => SyncManifestToRemote(indexWriter);
        }
        private Action<ExamineIndexWriter> _onCommitAction;
        public virtual void InvokeOnCommit(ExamineIndexWriter writer)
        {
            _onCommitAction?.Invoke(writer);
        }

        /// <summary>
        /// Whether to sync on commit
        /// </summary>
        public bool SyncOnCommit { get; set; }

        /// <summary>
        /// Called on commit
        /// </summary>
        /// <param name="action"></param>
        public void SetOnCommitAction(Action<ExamineIndexWriter> action)
        {
            _onCommitAction = action;
        }

        private Func<IndexWriter, MergePolicy> _mergePolicy;

        public virtual bool IsReadOnly { get; set; }
        public virtual MergePolicy GetMergePolicy(IndexWriter writer)
        {
            return _mergePolicy?.Invoke(writer);
        }

        public void SetMergePolicyAction(Func<IndexWriter, MergePolicy> policy)
        {
            _mergePolicy = policy;
        }

        private MergeScheduler MergeScheduler { get; set; }
        public virtual MergeScheduler GetMergeScheduler()
        {
            return MergeScheduler;
        }

        public void SetMergeScheduler(MergeScheduler policy)
        {
            MergeScheduler = policy;
        }
        private IndexDeletionPolicy DeletionPolicy { get; set; }
        public IndexDeletionPolicy GetDeletionPolicy() => DeletionPolicy;
        public void SetDeletion(IndexDeletionPolicy policy)
        {
            DeletionPolicy = policy;
        }
        public Lucene.Net.Store.Directory CacheDirectory { get; protected set; }

        public abstract string[] CheckDirtyWithoutWriter();

        public abstract void SetDirty();

        /// <summary>
        /// Syncs the local manifest to the remote directory
        /// </summary>
        /// <param name="writer"></param>
        public virtual void SyncManifestToRemote(ExamineIndexWriter writer)
        {
            if (!SyncOnCommit)
            {
                return;
            }
            //Lock
            var directoryLock = MakeLock(IndexWriter.WRITE_LOCK_NAME);
            bool obtained = false;
            try
            {
                obtained = directoryLock.Obtain();
                if (!obtained)
                {
                    //Well this is a problem
                    Trace.WriteLine("Unable to sync manifest. Failed to obtain directory lock");
                    return;
                }
                //Get last manifest
                var lastManifest = GetMostRecentManifest();
                //Generate file manifest.
                var manifest = GenerateManifest(lastManifest);

                //Upload files to remote store that have changed
                //Upload manifest and files
                UploadToRemote(manifest);

                //Purge old files on remote
                CleanupRemoteFiles();
            }
            finally
            {
                if (obtained)
                    directoryLock.Release();
                //Unlock
            }
        }

        protected abstract void CleanupRemoteFiles();

        protected abstract void UploadToRemote(ExamineDirectoryManifest manifest);

        protected abstract ExamineDirectoryManifest GetMostRecentManifest();

        public abstract string SerializeManifest(ExamineDirectoryManifest manifest);
        public abstract ExamineDirectoryManifest DeserializeManifest(string manifestText);

        protected abstract List<ExamineDirectoryManifest> GetAllManifests();

        protected virtual ExamineDirectoryManifest GenerateManifest(ExamineDirectoryManifest lastManifest = null)
        {
            var manifest = new ExamineDirectoryManifest();
            manifest.Id = Guid.NewGuid().ToString().Replace("-", "");
            var localFiles = CacheDirectory.ListAll();
            manifest.Modified = DateTime.UtcNow.Ticks;
            manifest.Entries = new List<ExamineDirectoryManifestEntry>();
            foreach (var fileName in localFiles)
            {
                ExamineDirectoryManifestEntry entry = GenerateManifestEntry(manifest, fileName);
                manifest.Entries.Add(entry);
            }
            return manifest;
        }

        protected virtual ExamineDirectoryManifestEntry GenerateManifestEntry(ExamineDirectoryManifest manifest, string fileName, ExamineDirectoryManifest lastManifest = null)
        {
            var entry = new ExamineDirectoryManifestEntry();
            entry.ModifiedDate = CacheDirectory.FileModified(fileName);
            entry.LuceneFileName = fileName;
            entry.Length = CacheDirectory.FileLength(fileName);
            entry.OriginalManifestId = manifest.Id;
            if (lastManifest != null && lastManifest.Entries != null)
            {
                var lastEntry = lastManifest.Entries.FirstOrDefault(x => entry.LuceneFileName.Equals(x.LuceneFileName));
                if (lastEntry != null && (lastEntry.Length == entry.Length
                        && lastEntry.ModifiedDate == entry.ModifiedDate))
                {
                    //Unchanged, use original file
                    entry.OriginalManifestId = lastEntry.OriginalManifestId;
                    entry.BlobFileName = lastEntry.BlobFileName;
                }
                else
                {
                    entry.BlobFileName = manifest.Id + "_" + entry.LuceneFileName;
                }
            }
            return entry;
        }
    }
}