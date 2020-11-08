using System;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.Directories
{
    public abstract class ExamineDirectory : Lucene.Net.Store.Directory
    {
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


        public MergeScheduler MergeScheduler;

       
    }
}