using System;
using Examine.Lucene.Search;
using Lucene.Net.Search;
using Lucene.Net.Analysis;


namespace Examine.Lucene.Providers
{

    ///<summary>
    /// Standard object used to search a Lucene index
    ///</summary>
    public class LuceneSearcher : BaseLuceneSearcher
    {
        #region Constructors

        /// <summary>
        /// Constructor allowing for creating a NRT instance based on a given writer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="writer"></param>
        /// <param name="analyzer"></param>
        /// <param name="fieldValueTypeCollection"></param>
        public LuceneSearcher(string name, SearcherManager searcherManager, Analyzer analyzer, FieldValueTypeCollection fieldValueTypeCollection)
            : base(name, analyzer)
        {
            _searcherManager = searcherManager;
            FieldValueTypeCollection = fieldValueTypeCollection;
        }

        #endregion      

        private readonly SearcherManager _searcherManager;
        public FieldValueTypeCollection FieldValueTypeCollection { get; }

        public override ISearchContext GetSearchContext()
            => new SearchContext(_searcherManager, FieldValueTypeCollection);
    }

}

