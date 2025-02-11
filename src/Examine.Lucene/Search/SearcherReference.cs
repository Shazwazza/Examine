using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <inheritdoc/>
    public readonly struct SearcherReference : ISearcherReference
    {
        private readonly SearcherManager _searcherManager;

        /// <inheritdoc/>
        public SearcherReference(SearcherManager searcherManager)
        {
            _searcherManager = searcherManager;
            IndexSearcher = _searcherManager.Acquire();
        }

        /// <inheritdoc/>
        public IndexSearcher IndexSearcher { get; }

        /// <inheritdoc/>
        public void Dispose() => _searcherManager.Release(IndexSearcher);
    }
}
