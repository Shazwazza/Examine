using System.Diagnostics;
using System.Security;
using Examine.SearchCriteria;
using Lucene.Net.Search;
using Examine.LuceneEngine.Providers;

namespace Examine.LuceneEngine.SearchCriteria
{
    /// <summary>
    /// An implementation of the fluent API boolean operations
    /// </summary>
    [DebuggerDisplay("{_search}")]
    public class LuceneBooleanOperation : IBooleanOperation
    {
        private readonly LuceneSearchCriteria _search;
        private bool _hasCompiled = false;

        internal LuceneBooleanOperation(LuceneSearchCriteria search)
        {
            this._search = search;
        }

        #region IBooleanOperation Members

        /// <summary>
        /// Sets the next operation to be AND
        /// </summary>
        /// <returns></returns>
        [SecuritySafeCritical]
        public IQuery And()
        {
            return new LuceneQuery(this._search, BooleanClause.Occur.MUST);
        }

        /// <summary>
        /// Sets the next operation to be OR
        /// </summary>
        /// <returns></returns>
		[SecuritySafeCritical]
        public IQuery Or()
        {
            return new LuceneQuery(this._search, BooleanClause.Occur.SHOULD);
        }

        /// <summary>
        /// Sets the next operation to be NOT
        /// </summary>
        /// <returns></returns>
		[SecuritySafeCritical]
        public IQuery Not()
        {
            return new LuceneQuery(this._search, BooleanClause.Occur.MUST_NOT);
        }

        /// <summary>
        /// Compiles this instance for fluent API conclusion
        /// </summary>
        /// <returns></returns>
		[SecuritySafeCritical]
        public ISearchCriteria Compile()
        {
            if (!_hasCompiled && !string.IsNullOrEmpty(this._search.SearchIndexType))
            {
                var query = this._search.Query;

                this._search.Query = new BooleanQuery();
                this._search.Query.Add(query, BooleanClause.Occur.MUST);

                //this.search.query.Add(this.search.queryParser.Parse("(" + query.ToString() + ")"), BooleanClause.Occur.MUST);

                this._search.FieldInternal(LuceneIndexer.IndexTypeFieldName, new ExamineValue(Examineness.Explicit, this._search.SearchIndexType.ToString()), BooleanClause.Occur.MUST);
                
                //ensure we don't compile twice!
                _hasCompiled = true;
            }
            
            return this._search;
        }

        #endregion
    }
}
