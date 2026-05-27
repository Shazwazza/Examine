using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    public readonly struct SearcherReference : ISearcherReference
    {
        private readonly SearcherManager _searcherManager;

        public SearcherReference(SearcherManager searcherManager)
        {
            _searcherManager = searcherManager;
            IndexSearcher = _searcherManager.Acquire();
        }

        public IndexSearcher IndexSearcher { get; }

        public void Dispose() => _searcherManager.Release(IndexSearcher);
    }
}
