using Lucene.Net.Search;

namespace Lucene.Net.Contrib.Management
{
    public interface ISearcherWarmer
    {
        void Warm(IndexSearcher s);        
    }
}