using System.Security;
using Examine.SearchCriteria;
using Lucene.Net.Search;
using Examine.LuceneEngine.Providers;

namespace Examine.LuceneEngine.SearchCriteria
{
    /// <summary>
    /// An implementation of the fluent API boolean operations
    /// </summary>
    public class LuceneBooleanOperation : IBooleanOperation
    {
        private LuceneSearchCriteria search;

        internal LuceneBooleanOperation(LuceneSearchCriteria search)
        {
            this.search = search;
        }

        #region IBooleanOperation Members

        /// <summary>
        /// Sets the next operation to be AND
        /// </summary>
        /// <returns></returns>
        [SecuritySafeCritical]
        public IQuery And()
        {
            return new LuceneQuery(this.search, BooleanClause.Occur.MUST);
        }

        /// <summary>
        /// Sets the next operation to be OR
        /// </summary>
        /// <returns></returns>
		[SecuritySafeCritical]
        public IQuery Or()
        {
            return new LuceneQuery(this.search, BooleanClause.Occur.SHOULD);
        }

        /// <summary>
        /// Sets the next operation to be NOT
        /// </summary>
        /// <returns></returns>
		[SecuritySafeCritical]
        public IQuery Not()
        {
            return new LuceneQuery(this.search, BooleanClause.Occur.MUST_NOT);
        }

        /// <summary>
        /// Compiles this instance for fluent API conclusion
        /// </summary>
        /// <returns></returns>
		[SecuritySafeCritical]
        public ISearchCriteria Compile()
        {
            if (!string.IsNullOrEmpty(this.search.SearchIndexType))
            {
                var query = this.search.Query;

                this.search.Query = new BooleanQuery();
                this.search.Query.Add(query, BooleanClause.Occur.MUST);

                //this.search.query.Add(this.search.queryParser.Parse("(" + query.ToString() + ")"), BooleanClause.Occur.MUST);

                this.search.FieldInternal(LuceneIndexer.IndexTypeFieldName, new ExamineValue(Examineness.Explicit, this.search.SearchIndexType.ToString()), BooleanClause.Occur.MUST);
            }
            
            return this.search;
        }

        #endregion
    }
}
