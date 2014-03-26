using Lucene.Net.Search;

namespace Examine.LuceneEngine.Cru
{
    public interface ISearcherWarmer
    {
        void Warm(IndexSearcher s);        
    }
}