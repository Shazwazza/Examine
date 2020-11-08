using Lucene.Net.Index;

namespace Examine.LuceneEngine.MergeShedulers
{
    internal class NoMergeSheduler  : MergeScheduler
    {
        public override void Merge(IndexWriter writer)
        {
        }

      
        protected override void Dispose(bool disposing)
        {
        }
    }
}