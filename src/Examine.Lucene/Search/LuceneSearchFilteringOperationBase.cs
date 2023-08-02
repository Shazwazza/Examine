using System;
using System.Collections.Generic;
using Examine.Search;
using Lucene.Net.Queries;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    // LuceneSearchQueryBase
    public abstract class LuceneSearchFilteringOperationBase : IFilter, INestedFilter
    {
        internal Stack<BooleanFilter> Filters => _luceneSearchQueryBase.Filters;

        /// <summary>
        /// The <see cref="BooleanFilter"/>
        /// </summary>
        internal BooleanFilter Filter => _luceneSearchQueryBase.Filters.Peek();

        private BooleanOperation _boolFilterOp;
        private readonly LuceneSearchQueryBase _luceneSearchQueryBase;

        /// <summary>
        /// Specifies how clauses are to occur in matching documents
        /// </summary>
        protected Occur Occurrence { get; set; }

        public LuceneSearchFilteringOperationBase(LuceneSearchQueryBase luceneSearchQueryBase)
        {
            _boolFilterOp = BooleanOperation.And;
            _luceneSearchQueryBase = luceneSearchQueryBase;
        }

        /// <summary>
        /// The type of boolean operation
        /// </summary>
        public BooleanOperation BooleanFilterOperation
        {
            get => _boolFilterOp;
            set
            {
                _boolFilterOp = value;
                Occurrence = _boolFilterOp.ToLuceneOccurrence();
            }
        }

        /// <summary>
        /// Adds a true Lucene Filter 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        public LuceneFilteringBooleanOperationBase LuceneFilter(Filter filter, BooleanOperation? op = null)
        {
            Filter.Add(filter, (op ?? BooleanFilterOperation).ToLuceneOccurrence());
            return CreateOp();
        }


        /// <summary>
        /// Creates a <see cref="LuceneFilteringBooleanOperationBase"/>
        /// </summary>
        /// <returns></returns>
        protected abstract LuceneFilteringBooleanOperationBase CreateOp();

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

        protected abstract INestedBooleanFilterOperation NestedChainFilters(Action<IFilterChainStart> chain);
        protected abstract INestedBooleanFilterOperation NestedTermFilter(FilterTerm term);
        protected abstract INestedBooleanFilterOperation NestedTermsFilter(IEnumerable<FilterTerm> terms);
        protected abstract INestedBooleanFilterOperation NestedTermPrefixFilter(FilterTerm term);
        protected abstract INestedBooleanFilterOperation NestedFieldValueExistsFilter(string field);
        protected abstract INestedBooleanFilterOperation NestedFieldValueNotExistsFilter(string field);
        protected abstract INestedBooleanFilterOperation NestedQueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp);
        protected abstract INestedBooleanFilterOperation NestedRangeFilter<T>(string field, T min, T max, bool minInclusive, bool maxInclusive);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedChainFilters(Action<IFilterChainStart> chain) => NestedChainFilters(chain);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedTermFilter(FilterTerm term) => NestedTermFilter(term);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedTermsFilter(IEnumerable<FilterTerm> terms) => NestedTermsFilter(terms);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedTermPrefix(FilterTerm term) => NestedTermPrefixFilter(term);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedFieldValueExists(string field) => NestedFieldValueExistsFilter(field);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedFieldValueNotExists(string field) => NestedFieldValueNotExistsFilter(field);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedQueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp) => NestedQueryFilter(inner, defaultOp);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedRangeFilter<T>(string field, T min, T max, bool minInclusive, bool maxInclusive) => NestedRangeFilter<T>(field, min, max, minInclusive, maxInclusive);
    }
}
