using System;
using System.Diagnostics;
using Examine.LuceneEngine.Providers;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Search
{
    /// <summary>
    /// An implementation of the fluent API boolean operations
    /// </summary>
    [DebuggerDisplay("{_search}")]
    public class LuceneBooleanOperation : IBooleanOperation
    {
        private readonly LuceneSearchQuery _search;
        
        internal LuceneBooleanOperation(LuceneSearchQuery search)
        {
            this._search = search;
        }

        #region IBooleanOperation Members

        /// <inheritdoc />
        public IQuery And()
        {
            return new LuceneQuery(this._search, Occur.MUST);
        }

        public IBooleanOperation And(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return Op(inner, BooleanOperation.And, defaultOp);
        }

        /// <inheritdoc />
        public IQuery Or()
        {
            return new LuceneQuery(this._search, Occur.SHOULD);
        }

        public IBooleanOperation Or(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return Op(inner, BooleanOperation.Or, defaultOp);
        }

        /// <inheritdoc />
        public IQuery Not()
        {
            return new LuceneQuery(this._search, Occur.MUST_NOT);
        }

        public IBooleanOperation AndNot(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return Op(inner, BooleanOperation.Not, defaultOp);
        }
        
        #endregion

        public ISearchResults Execute(int maxResults = 500)
        {
            return _search.Execute(maxResults);
        }

        #region IOrdering

        public IOrdering OrderBy(params SortableField[] fields)
        {
            return _search.OrderBy(fields);
        }

        public IOrdering OrderByDescending(params SortableField[] fields)
        {
            return _search.OrderByDescending(fields);
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

        public override string ToString()
        {
            return _search.ToString();
        }
    }
}
