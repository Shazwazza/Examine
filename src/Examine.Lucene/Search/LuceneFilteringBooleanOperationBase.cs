using System;
using System.Collections.Generic;
using Examine.Search;
using Lucene.Net.Queries;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Boolean Lucene Filtering Operation Base
    /// </summary>
    public abstract class LuceneFilteringBooleanOperationBase : IFilter, IBooleanFilterOperation, INestedBooleanFilterOperation
    {
        private readonly LuceneSearchFilteringOperationBase _search;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="luceneSearch"></param>
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
        internal LuceneFilteringBooleanOperationBase Op(
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
        /// <summary>
        /// Used to add a operation
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="outerOp"></param>
        /// <param name="defaultInnerOp"></param>
        /// <returns></returns>
        internal Filter GetNestedFilterOp(
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

            return _search.Filters.Pop();
        }

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

        /// <inheritdoc/>
        public abstract INestedFilter And();

        /// <inheritdoc/>
        public INestedBooleanFilterOperation And(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
            => Op(inner, BooleanOperation.And, defaultOp);

        /// <inheritdoc/>
        public abstract INestedFilter Or();

        /// <inheritdoc/>
        public INestedBooleanFilterOperation Or(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
            => Op(inner, BooleanOperation.Or, defaultOp);

        /// <inheritdoc/>
        public abstract INestedFilter Not();

        /// <inheritdoc/>
        public INestedBooleanFilterOperation AndNot(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
            => Op(inner, BooleanOperation.Not, defaultOp);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation IntRangeFilter(string field, int? min, int? max, bool minInclusive, bool maxInclusive);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation LongRangeFilter(string field, long? min, long? max, bool minInclusive, bool maxInclusive);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation FloatRangeFilter(string field, float? min, float? max, bool minInclusive, bool maxInclusive);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation DoubleRangeFilter(string field, double? min, double? max, bool minInclusive, bool maxInclusive);

        #endregion

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation SpatialOperationFilter(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IExamineSpatialShape> shape);
    }
}
