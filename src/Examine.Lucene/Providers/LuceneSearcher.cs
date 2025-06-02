using System;
using Examine.Lucene.Search;
using Lucene.Net.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Facet;

namespace Examine.Lucene.Providers
{

    ///<summary>
    /// Standard object used to search a Lucene index
    ///</summary>
    public class LuceneSearcher : BaseLuceneSearcher
    {
        private readonly SearcherManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private readonly bool _isNrt;
        private bool _disposedValue;
        private volatile ISearchContext? _searchContext;

        /// <summary>
        /// Constructor allowing for creating a NRT instance based on a given writer
        /// </summary>
        public LuceneSearcher(string name, SearcherManager searcherManager, Analyzer analyzer, FieldValueTypeCollection fieldValueTypeCollection, FacetsConfig facetsConfig, bool isNrt)
            : base(name, analyzer, facetsConfig)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection;
            _isNrt = isNrt;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public override void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        {
            if (!_disposedValue)
            {
                _searcherManager.Dispose();
                _disposedValue = true;
            }
        }
    }
}

