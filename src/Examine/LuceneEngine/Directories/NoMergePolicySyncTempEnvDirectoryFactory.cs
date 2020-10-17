using Examine.LuceneEngine.MergePolicies;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.Directories
{
    public class NoMergePolicySyncTempEnvDirectoryFactory : SyncTempEnvDirectoryFactory
    {
        public NoMergePolicySyncTempEnvDirectoryFactory()
        {
            
        }
        public MergePolicy GetMergePolicy(IndexWriter writer)
        {
            return new NoMergePolicy(writer);
        } 
    }
}