using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents a multi search searcher reference
    /// </summary>
    public class MultiSearchSearcherReference : ISearcherReference
    {
        /// <inheritdoc/>
        public MultiSearchSearcherReference(ISearcherReference[] inner)
        {
            _inner = inner;
        }

        private bool _disposedValue;
        private IndexSearcher? _searcher;
        private readonly ISearcherReference[] _inner;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach(var i in _inner)
                    {
                        i.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
#pragma warning disable IDE0022 // Use expression body for method
            Dispose(disposing: true);
#pragma warning restore IDE0022 // Use expression body for method
        }
    }
}
