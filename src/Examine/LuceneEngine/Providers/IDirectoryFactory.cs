using System.Security;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Providers
{
    public interface IDirectoryFactory
    {
        [SecuritySafeCritical]
        Directory CreateDirectory(LuceneIndexer indexer, string luceneIndexFolder);
    }
}