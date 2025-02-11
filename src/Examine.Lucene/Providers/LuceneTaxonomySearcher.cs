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
    public class LuceneTaxonomySearcher : BaseLuceneSearcher, IDisposable, ILuceneTaxonomySearcher
    {
        private readonly SearcherTaxonomyManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private readonly bool _isNrt;
        private bool _disposedValue;
        private volatile ITaxonomySearchContext _searchContext;

        /// <summary>
        /// Constructor allowing for creating a NRT instance based on a given writer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="searcherManager"></param>
        /// <param name="analyzer"></param>
        /// <param name="fieldValueTypeCollection"></param>
        /// <param name="facetsConfig"></param>
        public LuceneTaxonomySearcher(string name, SearcherTaxonomyManager searcherManager, Analyzer analyzer, FieldValueTypeCollection fieldValueTypeCollection, bool isNrt, FacetsConfig facetsConfig)
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

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _searcherManager.Dispose();
                }

                _disposedValue = true;
            }
            base.Dispose(disposing);
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
                IndexReader indexReader = indexSearcher.Searcher.IndexReader;
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

