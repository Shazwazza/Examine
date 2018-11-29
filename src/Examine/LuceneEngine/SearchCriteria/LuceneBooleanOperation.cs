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
        
        public IQuery And()
        {
            return new LuceneQuery(this._search, Occur.MUST);
        }

        public IBooleanOperation And(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return Op(inner, BooleanOperation.And, defaultOp);
        }

        /// <summary>
        /// Sets the next operation to be OR
        /// </summary>
        /// <returns></returns>

        public IQuery Or()
        {
            return new LuceneQuery(this._search, Occur.SHOULD);
        }

        public IBooleanOperation Or(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return Op(inner, BooleanOperation.Or, defaultOp);
        }

        /// <summary>
        /// Sets the next operation to be NOT
        /// </summary>
        /// <returns></returns>

        public IQuery Not()
        {
            return new LuceneQuery(this._search, Occur.MUST_NOT);
        }

        public IBooleanOperation AndNot(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return Op(inner, BooleanOperation.Not, defaultOp);
        }

        /// <summary>
        /// Compiles this instance for fluent API conclusion
        /// </summary>
        /// <returns></returns>

        public ISearchCriteria Compile()
        {
            if (!_hasCompiled)
            {
                var query = new BooleanQuery();

                query.Add(_search.Queries.Pop(), Occur.MUST);
                _search.Queries.Push(query);

                //this.search.query.Add(this.search.queryParser.Parse("(" + query.ToString() + ")"), Occur.MUST);

                if (!string.IsNullOrEmpty(this._search.Category))
                {
                    this._search.FieldInternal(LuceneIndex.CategoryFieldName, new ExamineValue(Examineness.Explicit, this._search.Category), Occur.MUST);
                }

                //ensure we don't compile twice!
                _hasCompiled = true;
            }

            return this._search;

            //if (!_hasCompiled && !string.IsNullOrEmpty(this._search.Category))
            //{
            //    var query = this._search.Query;

            //    this._search.Query = new BooleanQuery();
            //    this._search.Query.Add(query, Occur.MUST);

            //    //this.search.query.Add(this.search.queryParser.Parse("(" + query.ToString() + ")"), Occur.MUST);

            //    this._search.FieldInternal(
            //        LuceneIndexer.IndexTypeFieldName, new ExamineValue(Examineness.Explicit, this._search.Category), Occur.MUST,
            //        //Don't use the QueryParser to create this query, this is an explit query and Depending on the Query Parser used this could muck things up:
            //        // https://github.com/Shazwazza/Examine/issues/54
            //        false);

            //    //ensure we don't compile twice!
            //    _hasCompiled = true;
            //}

            //return this._search;
        }

        #endregion

        protected internal LuceneBooleanOperation Op(
            Func<IQuery, IBooleanOperation> inner,
            BooleanOperation outerOp,
            BooleanOperation? defaultInnerOp = null)
        {
            _search.Queries.Push(new BooleanQuery());

            //change the default inner op if specified
            var currentOp = _search.BooleanOperation;
            if (defaultInnerOp != null)
            {
                _search.BooleanOperation = defaultInnerOp.Value;
            }

            //run the inner search
            inner(_search);

            //reset to original op if specified
            if (defaultInnerOp != null)
            {
                _search.BooleanOperation = currentOp;
            }

            return _search.LuceneQuery(_search.Queries.Pop(), outerOp);
        }
    }
}
