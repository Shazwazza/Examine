using System.Collections;
using System.Security;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.MergePolicies
{
    internal class NoMergePolicy : MergePolicy
    {
        /// <summary>
        /// A singleton <see cref="T:Lucene.Net.Index.NoMergePolicy" /> which indicates the index does not use
        /// compound files.
        /// </summary>
        /// <summary>
        /// A singleton <see cref="T:Lucene.Net.Index.NoMergePolicy" /> which indicates the index uses compound
        /// files.
        /// </summary>
     
        private bool useCompoundFile  = true;
        private bool useCompoundDocStore = true;

        [SecurityCritical]
        public NoMergePolicy(IndexWriter writer) : base(writer)
        {
        }

        public override MergeSpecification FindMerges(SegmentInfos segmentInfos)
            => (MergePolicy.MergeSpecification) null;

        public override MergeSpecification FindMergesForOptimize(SegmentInfos segmentInfos, int maxSegmentCount, Hashtable segmentsToOptimize)
            => (MergePolicy.MergeSpecification) null;

        public override MergeSpecification FindMergesToExpungeDeletes(SegmentInfos segmentInfos)
            => (MergePolicy.MergeSpecification) null;

        public override void Close()
        {
        }
        public virtual bool GetUseCompoundFile() => this.useCompoundFile;

        public virtual void SetUseCompoundFile(bool useCompoundFile) => this.useCompoundFile = useCompoundFile;
        public override bool UseCompoundFile(SegmentInfos segments, SegmentInfo newSegment) => useCompoundFile;
        public virtual bool GetUseCompoundDocStore() => this.useCompoundDocStore;
        public virtual void SetUseCompoundDocStore(bool useCompoundDocStore) => this.useCompoundFile = useCompoundDocStore;
        public override bool UseCompoundDocStore(SegmentInfos segments) => useCompoundDocStore;
    }
}