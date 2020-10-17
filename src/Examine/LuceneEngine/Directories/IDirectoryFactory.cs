using System.Security;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Directories
{
    public interface IDirectoryFactory
    {
        [SecuritySafeCritical]
        Directory CreateDirectory(LuceneIndexer indexer, string luceneIndexFolder);

        bool IsReadOnly { get; }
        MergePolicy GetMergePolicy(IndexWriter writer);
    }
}