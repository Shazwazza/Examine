using System;
using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.Lucene.Providers
{
    /// <summary>
    /// A searcher for taxonomy indexes
    /// </summary>
    public class LuceneTaxonomySearcher : BaseLuceneSearcher, ILuceneTaxonomySearcher
    {
        private readonly SearcherTaxonomyManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private readonly bool _isNrt;
        private bool _disposedValue;
        private volatile ITaxonomySearchContext? _searchContext;

        /// <summary>
        /// Constructor allowing for creating a NRT instance based on a given writer
        /// </summary>
        public LuceneTaxonomySearcher(string name, SearcherTaxonomyManager searcherManager, Analyzer analyzer, FieldValueTypeCollection fieldValueTypeCollection, FacetsConfig facetsConfig, bool isNrt)
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
            var isCurrent = IsSearcherCurrent(_searcherManager);
            if (_searchContext is null || !isCurrent)
            {
                _searchContext = new TaxonomySearchContext(_searcherManager, _fieldValueTypeCollection, _isNrt);
            }

            return _searchContext;
        }

        /// <summary>
        /// Gets the Taxonomy SearchContext
        /// </summary>
        /// <returns></returns>
        public virtual ITaxonomySearchContext GetTaxonomySearchContext()
        {
            // Don't create a new search context unless something has changed
            var isCurrent = IsSearcherCurrent(_searcherManager);
            if (_searchContext is null || !isCurrent)
            {
                _searchContext = new TaxonomySearchContext(_searcherManager, _fieldValueTypeCollection, _isNrt);
            }

            return _searchContext;
        }

        /// <inheritdoc />
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public override void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        {
            if (!_disposedValue)
            {
                _searcherManager.Dispose();
                _disposedValue = true;
            }
            ;
        }

        /// <inheritdoc/>
        public int CategoryCount
        {
            get
            {
                var taxonomyReader = GetTaxonomySearchContext().GetTaxonomyAndSearcher().TaxonomyReader;
                return taxonomyReader.Count;
            }
        }

        /// <inheritdoc/>
        public int GetOrdinal(string dimension, string[] path)
        {
            var taxonomyReader = GetTaxonomySearchContext().GetTaxonomyAndSearcher().TaxonomyReader;
            return taxonomyReader.GetOrdinal(dimension, path);
        }


        /// <inheritdoc/>
        public IFacetLabel GetPath(int ordinal)
        {
            var taxonomyReader = GetTaxonomySearchContext().GetTaxonomyAndSearcher().TaxonomyReader;
            var facetLabel = taxonomyReader.GetPath(ordinal);
            var examineFacetLabel = new LuceneFacetLabel(facetLabel);
            return examineFacetLabel;
        }

        //
        // Summary:
        //     Returns true if no changes have occured since this searcher ie. reader was opened,
        //     otherwise false.
        private bool IsSearcherCurrent(SearcherTaxonomyManager searcherTaxonomyManager)
        {
            var indexSearcher = searcherTaxonomyManager.Acquire();
            try
            {
                var indexReader = indexSearcher.Searcher.IndexReader;
                //if (Debugging.AssertsEnabled)
                //{
                //    Debugging.Assert(indexReader is DirectoryReader, "searcher's IndexReader should be a DirectoryReader, but got {0}", indexReader);
                //}

                return ((DirectoryReader)indexReader).IsCurrent();
            }
            finally
            {
                searcherTaxonomyManager.Release(indexSearcher);
            }
        }
    }
}

