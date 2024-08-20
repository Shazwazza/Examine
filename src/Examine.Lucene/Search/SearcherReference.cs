using System;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    // TODO: struct
    public readonly struct SearcherReference : ISearcherReference
    {
        //private bool _disposedValue;
        private readonly SearcherManager _searcherManager;
        private readonly IndexSearcher _searcher;

        public SearcherReference(SearcherManager searcherManager)
        {
            _searcherManager = searcherManager;
            _searcher = _searcherManager.Acquire();
        }

        public IndexSearcher IndexSearcher
        {
            get
            {
                //if (_disposedValue)
                //{
                //    throw new ObjectDisposedException($"{nameof(SearcherReference)} is disposed");
                //}

                //return _searcher ??= _searcherManager.Acquire();
                return _searcher;
            }
        }

        public void Dispose()
        {
            //if (!_disposedValue)
            //{
                //if (_searcher != null)
                //{
                _searcherManager.Release(_searcher);
                //}
            //    _disposedValue = true;
            //}
        }
    }
}
