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
        private Action<ExamineIndexWriter> _onCommitAction;
        public virtual void InvokeOnCommit(ExamineIndexWriter writer)
        {
            _onCommitAction?.Invoke(writer);
        }

        public event EventHandler HandleOutOfSyncEvent;

        public void HandleOutOfSyncDirectory()
        {
            HandleOutOfSyncEvent?.Invoke(this, null);
        }

        /// <summary>
        /// Called on commit
        /// </summary>
        /// <param name="action"></param>
        public void SetOnCommitAction(Action<ExamineIndexWriter> action)
        {
            _onCommitAction = action;
        }

        private Func<IndexWriter, MergePolicy> _mergePolicy;

        public bool IsReadOnly { get; set; }
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

        public abstract string[] CheckDirtyWithoutWriter();

        public abstract void SetDirty();
        public object RebuildLock = new object();

    }
}