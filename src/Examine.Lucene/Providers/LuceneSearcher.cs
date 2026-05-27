using System;
using Examine.Lucene.Search;
using Lucene.Net.Search;
using Lucene.Net.Analysis;

namespace Examine.Lucene.Providers
{

    ///<summary>
    /// Standard object used to search a Lucene index
    ///</summary>
    public class LuceneSearcher : BaseLuceneSearcher, IDisposable
    {
        private readonly SearcherManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private readonly bool _isNrt;
        private bool _disposedValue;
        private volatile ISearchContext _searchContext;

        [Obsolete("Use ctor with all dependencies")]
        public LuceneSearcher(string name, SearcherManager searcherManager, Analyzer analyzer, FieldValueTypeCollection fieldValueTypeCollection)
            : base(name, analyzer)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection;
        }

        public LuceneSearcher(string name, SearcherManager searcherManager, Analyzer analyzer, FieldValueTypeCollection fieldValueTypeCollection, bool isNrt)
            : base(name, analyzer)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection;
            _isNrt = isNrt;
        }

        public override ISearchContext GetSearchContext()
        {
            // Don't create a new search context unless something has changed
            var isCurrent = _searcherManager.IsSearcherCurrent();
            if (_searchContext is null || !isCurrent)
            {
                _searchContext = new SearchContext(_searcherManager, _fieldValueTypeCollection, _isNrt);
            }

            return _searchContext;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _searcherManager.Dispose();
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

