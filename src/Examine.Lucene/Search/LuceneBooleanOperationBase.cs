using System;
using System.Collections.Generic;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    public abstract class LuceneBooleanOperationBase : IBooleanOperation, INestedBooleanOperation, IOrdering, IFaceting
    {
        private readonly LuceneSearchQueryBase _search;

        protected LuceneBooleanOperationBase(LuceneSearchQueryBase search)
        {
            _search = search;
        }

        public abstract IQuery And();
        public abstract IQuery Or();
        public abstract IQuery Not();

        public IBooleanOperation And(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And) 
            => Op(inner, BooleanOperation.And, defaultOp);

        public IBooleanOperation Or(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And) 
            => Op(inner, BooleanOperation.Or, defaultOp);

        public IBooleanOperation AndNot(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And) 
            => Op(inner, BooleanOperation.Not, defaultOp);

        protected abstract INestedQuery AndNested();
        protected abstract INestedQuery OrNested();
        protected abstract INestedQuery NotNested();

        INestedQuery INestedBooleanOperation.And() => AndNested();
        INestedQuery INestedBooleanOperation.Or() => OrNested();
        INestedQuery INestedBooleanOperation.Not() => NotNested();

        INestedBooleanOperation INestedBooleanOperation.And(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp) 
            => Op(inner, BooleanOperation.And, defaultOp);

        INestedBooleanOperation INestedBooleanOperation.Or(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp)
            => Op(inner, BooleanOperation.Or, defaultOp);

        INestedBooleanOperation INestedBooleanOperation.AndNot(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp)
            => Op(inner, BooleanOperation.Not, defaultOp);

        protected internal LuceneBooleanOperationBase Op(
            Func<INestedQuery, INestedBooleanOperation> inner,
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

        public abstract ISearchResults Execute(QueryOptions options = null);
        public abstract IOrdering OrderBy(params SortableField[] fields);
        public abstract IOrdering OrderByDescending(params SortableField[] fields);

        public abstract IOrdering SelectFields(ISet<string> fieldNames);
        public abstract IOrdering SelectField(string fieldName);
        public abstract IOrdering SelectAllFields();

        public abstract IQueryExecutor WithFacets(Action<IFacetOperations> facets);
    }
}
