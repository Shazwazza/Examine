using System;
using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Taxonomy;

namespace Examine.Lucene.Providers
{
    /// <summary>
    /// A searcher for taxonomy indexes
    /// </summary>
    public class LuceneTaxonomySearcher : BaseLuceneSearcher, IDisposable, ILuceneTaxonomySearcher
    {
        private readonly SearcherTaxonomyManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private readonly SimilarityDefinitionCollection _similarityDefinitionCollection;
        private bool _disposedValue;

        /// <summary>
        /// Constructor allowing for creating a NRT instance based on a given writer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="searcherManager"></param>
        /// <param name="analyzer"></param>
        /// <param name="fieldValueTypeCollection"></param>
        /// <param name="facetsConfig"></param>
        public LuceneTaxonomySearcher(string name, SearcherTaxonomyManager searcherManager, Analyzer analyzer, FieldValueTypeCollection fieldValueTypeCollection, FacetsConfig facetsConfig)
            : base(name, analyzer, facetsConfig)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection;
        }

        /// <summary>
        /// Constructor allowing for creating a NRT instance based on a given writer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="searcherManager"></param>
        /// <param name="analyzer"></param>
        /// <param name="fieldValueTypeCollection"></param>
        /// <param name="facetsConfig"></param>
        /// <param name="similarityDefinitions"></param>
        public LuceneTaxonomySearcher(string name, SearcherTaxonomyManager searcherManager, Analyzer analyzer, FieldValueTypeCollection fieldValueTypeCollection, FacetsConfig facetsConfig, SimilarityDefinitionCollection similarityDefinitions)
            : base(name, analyzer, facetsConfig)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection;
            _similarityDefinitionCollection = similarityDefinitions;
        }

        /// <inheritdoc/>
        public override ISearchContext GetSearchContext()
            => new TaxonomySearchContext(_searcherManager, _fieldValueTypeCollection, _similarityDefinitionCollection);

        /// <summary>
        /// Gets the Taxonomy SearchContext
        /// </summary>
        /// <returns></returns>
        public virtual ITaxonomySearchContext GetTaxonomySearchContext()
            => new TaxonomySearchContext(_searcherManager, _fieldValueTypeCollection, _similarityDefinitionCollection);

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
    }
}

