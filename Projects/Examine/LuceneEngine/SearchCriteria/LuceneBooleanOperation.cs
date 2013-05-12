using System;
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

        public IBooleanOperation And(Func<IQuery, IBooleanOperation> inner)
        {
            return Op(inner, BooleanOperation.And);
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

        public IBooleanOperation Or(Func<IQuery, IBooleanOperation> inner)
        {
            return Op(inner, BooleanOperation.Or);
        }

        protected IBooleanOperation Op(Func<IQuery, IBooleanOperation> inner, BooleanOperation op)
        {
            _search.Queries.Push(new BooleanQuery());
            inner(_search);
            return _search.LuceneQuery(_search.Queries.Pop(), op);            
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


        public IBooleanOperation AndNot(Func<IQuery, IBooleanOperation> inner)
        {
            return Op(inner, BooleanOperation.Not);
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

                var query = new BooleanQuery();

                query.Add(_search.Queries.Pop(), BooleanClause.Occur.MUST);
                _search.Queries.Push(query);

                //this.search.query.Add(this.search.queryParser.Parse("(" + query.ToString() + ")"), BooleanClause.Occur.MUST);

                this._search.FieldInternal(LuceneIndexer.IndexTypeFieldName, new ExamineValue(Examineness.Explicit, this._search.SearchIndexType.ToString()), BooleanClause.Occur.MUST);
                
                
                //ensure we don't compile twice!
                _hasCompiled = true;
            }
            
            return this._search;
        }

        public ISearchResults Execute()
        {
            return _search.Execute();
        }

        #endregion
    }
}
