using System;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Search
{
    public abstract class LuceneBooleanOperationBase : IBooleanOperation
    {
        private readonly LuceneSearchQueryBase _search;

        protected LuceneBooleanOperationBase(LuceneSearchQueryBase search)
        {
            _search = search;
        }

        public abstract IQuery And();

        public IBooleanOperation And(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return Op(inner, BooleanOperation.And, defaultOp);
        }

        public abstract IQuery Or();

        public IBooleanOperation Or(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return Op(inner, BooleanOperation.Or, defaultOp);
        }

        public abstract IQuery Not();

        public IBooleanOperation AndNot(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return Op(inner, BooleanOperation.Not, defaultOp);
        }

        protected internal IBooleanOperation Op(
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

        public abstract ISearchResults Execute(int maxResults = 500);
        public abstract IOrdering OrderBy(params SortableField[] fields);
        public abstract IOrdering OrderByDescending(params SortableField[] fields);
    }
}