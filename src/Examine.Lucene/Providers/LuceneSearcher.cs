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
    public class LuceneSearcher : BaseLuceneSearcher, IDisposable
    {
        private readonly SearcherManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private bool _disposedValue;


        /// <summary>
        /// Constructor allowing for creating a NRT instance based on a given writer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="searcherManager"></param>
        /// <param name="analyzer"></param>
        /// <param name="fieldValueTypeCollection"></param>
        [Obsolete("To remove in Examine V5")]
        public LuceneSearcher(string name, SearcherManager searcherManager, Analyzer analyzer, FieldValueTypeCollection fieldValueTypeCollection)
            : base(name, analyzer)
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
        public LuceneSearcher(string name, SearcherManager searcherManager, Analyzer analyzer, FieldValueTypeCollection fieldValueTypeCollection, FacetsConfig facetsConfig)
            : base(name, analyzer, facetsConfig)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection;
        }

        /// <inheritdoc/>
        public override ISearchContext GetSearchContext()
            => new SearchContext(_searcherManager, _fieldValueTypeCollection);

        /// <inheritdoc/>
        [Obsolete("To remove in Examine v5")]
        protected new virtual void Dispose(bool disposing)
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
        [Obsolete("To remove in Examine V5 - IDisposable is implemented in base class")]
        public new void Dispose() => Dispose(true);
    }
}

