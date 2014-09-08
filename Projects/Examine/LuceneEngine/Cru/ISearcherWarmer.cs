using Lucene.Net.Search;

namespace Examine.LuceneEngine.Cru
{
    internal interface ISearcherWarmer
    {
        void Warm(IndexSearcher s);        
    }
}