using System;
using Examine.Lucene.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Taxonomy;

namespace Examine.Lucene.Providers
{
    public class LuceneTaxonomySearcher : BaseLuceneSearcher, IDisposable, ILuceneTaxonomySearcher
    {
        private readonly SearcherTaxonomyManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private bool _disposedValue;

        /// <summary>
        /// Constructor allowing for creating a NRT instance based on a given writer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="writer"></param>
        /// <param name="analyzer"></param>
        /// <param name="fieldValueTypeCollection"></param>
        public LuceneTaxonomySearcher(string name, SearcherTaxonomyManager searcherManager, Analyzer analyzer, FieldValueTypeCollection fieldValueTypeCollection, FacetsConfig facetsConfig)
            : base(name, analyzer, facetsConfig)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection;
        }

        public override ISearchContext GetSearchContext()
            => new TaxonomySearchContext(_searcherManager, _fieldValueTypeCollection);


        public virtual ITaxonomySearchContext GetTaxonomySearchContext()
            => new TaxonomySearchContext(_searcherManager, _fieldValueTypeCollection);

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

        /// <inheritdoc/>
        public long CategoryCount
        {
            get
            {
                var taxonomyReader = GetTaxonomySearchContext().GetTaxonomyAndSearcher().TaxonomyReader;
                return taxonomyReader.Count;
            }
        }

        /// <inheritdoc/>
        public int GetOrdinal(string dim, string[] path)
        {
            var taxonomyReader = GetTaxonomySearchContext().GetTaxonomyAndSearcher().TaxonomyReader;
            return taxonomyReader.GetOrdinal(dim, path);
        }
    }
}

