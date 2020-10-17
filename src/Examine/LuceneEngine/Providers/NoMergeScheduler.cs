using System.Security;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.Providers
{   
    [SecurityCritical]
    internal class NoMergeScheduler  : MergeScheduler
    {
        [SecurityCritical]
        public NoMergeScheduler()
        {
        }
        [SecurityCritical]
        public override void Merge(IndexWriter writer)
        {
        }
        [SecurityCritical]
        public override void Close()
        {
        }
    }
}