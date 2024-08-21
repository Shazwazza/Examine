using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    public class MultiSearchSearcherReference : ISearcherReference
    {
        public MultiSearchSearcherReference(ISearcherReference[] inner)
        {
            _inner = inner;
        }

        private bool _disposedValue;
        private IndexSearcher _searcher;
        private readonly ISearcherReference[] _inner;

        public IndexSearcher IndexSearcher
        {
            get
            {
                if (_searcher == null)
                {
                    var searchables = new IndexReader[_inner.Length];
                    for (int i = 0; i < _inner.Length; i++)
                    {
                        searchables[i] = _inner[i].IndexSearcher.IndexReader;
                    }
                    _searcher = new IndexSearcher(new MultiReader(searchables));
                }
                return _searcher;

            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var i in _inner)
                    {
                        i.Dispose();
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
