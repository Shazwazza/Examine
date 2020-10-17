using Lucene.Net.Index;

namespace Examine.LuceneEngine.MergeShedulers
{
    internal class NoMergeSheduler  : MergeScheduler
    {
        public override void Merge(IndexWriter writer)
        {
        }

        public override void Close()
        {
        }
    }
}