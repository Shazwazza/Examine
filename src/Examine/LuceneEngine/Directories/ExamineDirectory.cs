using System;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using static Lucene.Net.Index.IndexWriter;

namespace Examine.LuceneEngine.Directories
{
    public abstract class ExamineDirectory : Lucene.Net.Store.Directory
    {
        private Func<IndexWriter, MergePolicy> _mergePolicy;
        private Func<Directory, Analyzer, bool, MaxFieldLength, BaseIndexProvider, IndexWriter> _indexWriterFactory;

        public virtual bool IsReadOnly { get; set; }
        public virtual MergePolicy GetMergePolicy(IndexWriter writer)
        {
            return _mergePolicy?.Invoke(writer);
        }

        public void SetMergePolicyAction(Func<IndexWriter, MergePolicy> policy)
        {
            _mergePolicy = policy;
        }


        private MergeScheduler MergeScheduler {  get;  set; }
        public virtual MergeScheduler GetMergeScheduler()
        {
            return MergeScheduler;
        }

        public void SetMergeScheduler(MergeScheduler policy)
        {
            MergeScheduler = policy;
        }
        private IndexDeletionPolicy DeletionPolicy {  get;  set; }
        public IndexDeletionPolicy GetDeletionPolicy() => DeletionPolicy;
        public void SetDeletion(IndexDeletionPolicy policy)
        {
            DeletionPolicy = policy;
        }

        public virtual void SetIndexWriterFactory(Func<Directory, Analyzer, bool, MaxFieldLength, BaseIndexProvider, IndexWriter> factory)
        {
            _indexWriterFactory = factory;
        }
        public virtual IndexWriter GetIndexWriter(Analyzer analyzer,bool create, MaxFieldLength maxFieldLength,BaseIndexProvider index)
        {
            return _indexWriterFactory?.Invoke(this,analyzer,create, maxFieldLength,index);
        }
    }
}