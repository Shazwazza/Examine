using System.Collections;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.MergePolicies
{
    public class NoMergePolicy : MergePolicy
    {
        public NoMergePolicy(IndexWriter writer) : base(writer)
        {
        }

        public override MergeSpecification FindMerges(SegmentInfos segmentInfos)
        {
            throw new System.NotImplementedException();
        }

        public override MergeSpecification FindMergesForOptimize(SegmentInfos segmentInfos, int maxSegmentCount, Hashtable segmentsToOptimize)
        {
            throw new System.NotImplementedException();
        }

        public override MergeSpecification FindMergesToExpungeDeletes(SegmentInfos segmentInfos)
        {
            throw new System.NotImplementedException();
        }

        public override void Close()
        {
            throw new System.NotImplementedException();
        }

        public override bool UseCompoundFile(SegmentInfos segments, SegmentInfo newSegment)
        {
            throw new System.NotImplementedException();
        }

        public override bool UseCompoundDocStore(SegmentInfos segments)
        {
            throw new System.NotImplementedException();
        }
    }
}