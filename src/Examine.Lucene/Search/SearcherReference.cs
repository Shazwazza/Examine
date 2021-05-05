using System;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    public class SearcherReference : ISearcherReference
    {
        private bool _disposedValue;
        private readonly SearcherManager _searcherManager;
        private IndexSearcher _searcher;

        public SearcherReference(SearcherManager searcherManager)
        {
            _searcherManager = searcherManager;
        }

        public IndexSearcher IndexSearcher
        {
            get
            {
                if (_disposedValue)
                {
                    throw new ObjectDisposedException($"{nameof(SearcherReference)} is disposed");
                }
                return _searcher ?? (_searcher = _searcherManager.Acquire());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_searcher != null)
                    {
                        _searcherManager.Release(_searcher);
                    } 
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
