using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Microsoft.Extensions.Options;

namespace Examine.Lucene.Providers
{
    /// <summary>
    /// A searcher for indexes without taxonomy support
    /// </summary>
    internal class LuceneNonTaxonomySearcher : BaseLuceneSearcher
    {
        private readonly SearcherManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private readonly bool _isNrt;
        private bool _disposedValue;
        private volatile ISearchContext? _searchContext;

        /// <summary>
        /// Constructor allowing for creating a NRT instance based on a given writer
        /// </summary>
        public LuceneNonTaxonomySearcher(string name, SearcherManager searcherManager, FieldValueTypeCollection fieldValueTypeCollection, IOptionsMonitor<LuceneSearcherOptions> options, bool isNrt)
            : base(name, options)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection;
            _isNrt = isNrt;
        }

        /// <inheritdoc/>
        public override ISearchContext GetSearchContext()
        {
            // Don't create a new search context unless something has changed
            var isCurrent = IsSearcherCurrent(_searcherManager);
            if (_searchContext is null || !isCurrent)
            {
                _searchContext = new SearchContext(_searcherManager, _fieldValueTypeCollection, _isNrt);
            }

            return _searchContext;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            if (!_disposedValue)
            {
                _searcherManager.Dispose();
                _disposedValue = true;
            }
        }

        //
        // Summary:
        //     Returns true if no changes have occured since this searcher ie. reader was opened,
        //     otherwise false.
        private bool IsSearcherCurrent(SearcherManager searcherManager)
        {
            var indexSearcher = searcherManager.Acquire();
            try
            {
                var indexReader = indexSearcher.IndexReader;
                return ((DirectoryReader)indexReader).IsCurrent();
            }
            finally
            {
                searcherManager.Release(indexSearcher);
            }
        }
    }
}
