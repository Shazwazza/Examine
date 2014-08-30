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

        /// <summary>
        /// Sets the next operation to be AND
        /// </summary>
        /// <returns></returns>
        
        IQuery IBooleanOperation.And()
        {
            return And();
        }

        public LuceneBooleanOperation And(Func<LuceneQuery, LuceneBooleanOperation> inner)
        {
            //TODO: Test this!!
            return Op(query => inner((LuceneQuery)query), BooleanOperation.And);
        }

        public LuceneQuery Or()
        {
            return new LuceneQuery(this._search, BooleanClause.Occur.SHOULD);
        }

        public LuceneBooleanOperation Or(Func<LuceneQuery, LuceneBooleanOperation> inner)
        {
            //TODO: Test this!!
            return Op(query => inner((LuceneQuery)query), BooleanOperation.Or);
        }

        public LuceneQuery Not()
        {
            return new LuceneQuery(this._search, BooleanClause.Occur.MUST_NOT);
        }

        public LuceneBooleanOperation AndNot(Func<LuceneQuery, LuceneBooleanOperation> inner)
        {
            //TODO: Test this!!
            return Op(query => inner((LuceneQuery)query), BooleanOperation.Not);
        }

        public LuceneQuery And()
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
		
        IQuery IBooleanOperation.Or()
        {
            return Or();
        }

        public IBooleanOperation Or(Func<IQuery, IBooleanOperation> inner)
        {
            return Op(inner, BooleanOperation.Or);
        }

        /// <summary>
        /// Sets the next operation to be NOT
        /// </summary>
        /// <returns></returns>

        IQuery IBooleanOperation.Not()
        {
            return Not();
        }


        public IBooleanOperation AndNot(Func<IQuery, IBooleanOperation> inner)
        {
            return Op(inner, BooleanOperation.Not);
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

        #endregion

        protected LuceneBooleanOperation Op(Func<IQuery, IBooleanOperation> inner, BooleanOperation op)
        {
            _search.Queries.Push(new BooleanQuery());
            inner(_search);
            return _search.LuceneQuery(_search.Queries.Pop(), op);
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
