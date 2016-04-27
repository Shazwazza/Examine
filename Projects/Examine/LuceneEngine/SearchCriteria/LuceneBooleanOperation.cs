using System;
using System.Diagnostics;
using System.Security;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Scoring;
using Examine.SearchCriteria;
using Lucene.Net.Search;
using Examine.LuceneEngine.Providers;

namespace Examine.LuceneEngine.SearchCriteria
{
    /// <summary>
    /// An implementation of the fluent API boolean operations
    /// </summary>
    [DebuggerDisplay("{_search}")]
    public class LuceneBooleanOperation : IBooleanOperation<LuceneBooleanOperation, LuceneQuery, LuceneSearchCriteria>
    {
        private readonly LuceneSearchCriteria _search;
        private bool _hasCompiled = false;

        internal LuceneBooleanOperation(LuceneSearchCriteria search)
        {
            this._search = search;
        }

        #region IBooleanOperation Members

        //public LuceneBooleanOperation Group(Func<LuceneQuery, LuceneBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        //{
        //    return Op(query => inner((LuceneQuery)query), defaultOp, defaultOp);
        //}

        /// <summary>
        /// Sets the next operation to be AND
        /// </summary>
        /// <returns></returns>
        
        IQuery IBooleanOperation.And()
        {
            return And();
        }

        public LuceneBooleanOperation And(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return Op(inner, BooleanOperation.And, defaultOp);
        }

        public LuceneQuery Or()
        {
            return new LuceneQuery(this._search, Occur.SHOULD);
        }

        public LuceneBooleanOperation Or(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return Op(inner, BooleanOperation.Or, defaultOp);
        }

        public LuceneQuery Not()
        {
            return new LuceneQuery(this._search, Occur.MUST_NOT);
        }

        public LuceneBooleanOperation AndNot(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return Op(inner, BooleanOperation.Not, defaultOp);
        }

        public LuceneQuery And()
        {
            return new LuceneQuery(this._search, Occur.MUST);
        }

        IBooleanOperation IBooleanOperation.And(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return Op(inner, BooleanOperation.And, defaultOp);
        }

        /// <summary>
        /// Sets the next operation to be OR
        /// </summary>
        /// <returns></returns>
		
        IQuery IBooleanOperation.Or()
        {
            return Or();
        }

        IBooleanOperation IBooleanOperation.Or(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return Op(inner, BooleanOperation.Or, defaultOp);
        }

        /// <summary>
        /// Sets the next operation to be NOT
        /// </summary>
        /// <returns></returns>

        IQuery IBooleanOperation.Not()
        {
            return Not();
        }


        IBooleanOperation IBooleanOperation.AndNot(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return Op(inner, BooleanOperation.Not, defaultOp);
        }

        ISearchCriteria IBooleanOperation.Compile()
        {
            return Compile();
        }


        /// <summary>
        /// Compiles this instance for fluent API conclusion
        /// </summary>
        /// <returns></returns>		
        public LuceneSearchCriteria Compile()
        {
            if (!_hasCompiled)
            {                
                var query = new BooleanQuery();

                query.Add(_search.Queries.Pop(), Occur.MUST);
                _search.Queries.Push(query);

                //this.search.query.Add(this.search.queryParser.Parse("(" + query.ToString() + ")"), Occur.MUST);

                if (!string.IsNullOrEmpty(this._search.SearchIndexType))
                {
                    this._search.FieldInternal(LuceneIndexer.IndexTypeFieldName, new ExamineValue(Examineness.Explicit, this._search.SearchIndexType.ToString()), Occur.MUST);
                }
                
                //ensure we don't compile twice!
                _hasCompiled = true;
            }
            
            return this._search;
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

        public LuceneSearchCriteria WrapRelevanceScore(ScoreOperation op, params IFacetLevel[] levels)
        {
            return this.Compile().WrapRelevanceScore(op, levels);
        }
        
        public LuceneSearchCriteria WrapExternalDataScore<TData>(ScoreOperation op, Func<TData, float> scorer)
            where TData : class
        {
            return this.Compile().WrapExternalDataScore<TData>(op, scorer);
        }
    }
}
