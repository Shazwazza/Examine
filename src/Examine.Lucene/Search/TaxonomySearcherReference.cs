using System;
using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Facet.Taxonomy.Directory;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents a Taxonomy Searcher Reference
    /// </summary>
    public class TaxonomySearcherReference : ITaxonomySearcherReference
    {
        private bool _disposedValue;
        private readonly SearcherTaxonomyManager _searcherManager;
        private SearcherTaxonomyManager.SearcherAndTaxonomy? _searcherAndTaxonomy;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="searcherManager">Taxonomny Searcher Manager</param>
        public TaxonomySearcherReference(SearcherTaxonomyManager searcherManager)
        {
            _searcherManager = searcherManager ?? throw new ArgumentNullException(nameof(searcherManager));
        }

        /// <inheritdoc/>
        public IndexSearcher IndexSearcher
        {
            get
            {
                if (_disposedValue)
                {
                    throw new ObjectDisposedException($"{nameof(TaxonomySearcherReference)} is disposed");
                }
                return _searcherAndTaxonomy?.Searcher ?? (_searcherAndTaxonomy = _searcherManager.Acquire()).Searcher;
            }
        }

        /// <inheritdoc/>
        public DirectoryTaxonomyReader TaxonomyReader
        {
            get
            {
                if (_disposedValue)
                {
                    throw new ObjectDisposedException($"{nameof(TaxonomySearcherReference)} is disposed");
                }
                return _searcherAndTaxonomy?.TaxonomyReader ?? (_searcherAndTaxonomy = _searcherManager.Acquire()).TaxonomyReader;
            }
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_searcherAndTaxonomy != null)
                    {
                        _searcherManager.Release(_searcherAndTaxonomy);
                    }
                }

                _disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
