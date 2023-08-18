using System;
using System.Collections.Generic;
using Examine.Search;
using Lucene.Net.Queries;

namespace Examine.Lucene.Search
{
    public abstract class LuceneFilteringBooleanOperationBase : IFilter, IBooleanFilterOperation, INestedBooleanFilterOperation
    {
        private readonly LuceneSearchFilteringOperationBase _search;

        public LuceneFilteringBooleanOperationBase(LuceneSearchFilteringOperationBase luceneSearch)
        {
            _search = luceneSearch;
        }

        /// <summary>
        /// Used to add a operation
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="outerOp"></param>
        /// <param name="defaultInnerOp"></param>
        /// <returns></returns>
        protected internal LuceneFilteringBooleanOperationBase Op(
            Func<INestedFilter, INestedBooleanFilterOperation> inner,
            BooleanOperation outerOp,
            BooleanOperation? defaultInnerOp = null)
        {
            _search.Filters.Push(new BooleanFilter());

            //change the default inner op if specified
            var currentOp = _search.BooleanFilterOperation;
            if (defaultInnerOp != null)
            {
                _search.BooleanFilterOperation = defaultInnerOp.Value;
            }

            //run the inner search
            inner(_search);

            //reset to original op if specified
            if (defaultInnerOp != null)
            {
                _search.BooleanFilterOperation = currentOp;
            }

            return _search.LuceneFilter(_search.Filters.Pop(), outerOp);
        }


        /// <inheritdoc/>
        public abstract IBooleanFilterOperation ChainFilters(Action<IFilterChainStart> chain);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation TermFilter(FilterTerm term);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation TermsFilter(IEnumerable<FilterTerm> terms);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation TermPrefixFilter(FilterTerm term);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation FieldValueExistsFilter(string field);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation FieldValueNotExistsFilter(string field);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation QueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation RangeFilter<T>(string field, T min, T max, bool minInclusive = true, bool maxInclusive = true) where T : struct;

        #region IBooleanFilterOperation

        /// <inheritdoc/>
        public abstract IFilter AndFilter();

        /// <inheritdoc/>
        public IBooleanFilterOperation AndFilter(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
            => Op(inner, BooleanOperation.And, defaultOp);

        /// <inheritdoc/>
        public abstract IFilter OrFilter();

        /// <inheritdoc/>
        public IBooleanFilterOperation OrFilter(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
            => Op(inner, BooleanOperation.Or, defaultOp);

        /// <inheritdoc/>
        public abstract IFilter NotFilter();

        /// <inheritdoc/>
        public IBooleanFilterOperation AndNotFilter(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
            => Op(inner, BooleanOperation.Not, defaultOp);
        public abstract INestedFilter And();
        public  INestedBooleanFilterOperation And(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
            => Op(inner, BooleanOperation.And, defaultOp);
        public abstract INestedFilter Or();
        public INestedBooleanFilterOperation Or(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
            => Op(inner, BooleanOperation.Or, defaultOp);
        public abstract INestedFilter Not();
        public INestedBooleanFilterOperation AndNot(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
            => Op(inner, BooleanOperation.Not, defaultOp);

        #endregion
    }
}
