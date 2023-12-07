using System;
using System.Collections.Generic;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Lucene Filter
    /// </summary>
    public class LuceneFilter: IFilter, INestedFilter
    {
        private readonly LuceneSearchFilteringOperation _search;

        private readonly Occur _occurrence;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuceneFilter"/> class.
        /// </summary>
        /// <param name="search">The filter.</param>
        /// <param name="occurrence">The occurance.</param>
        public LuceneFilter(LuceneSearchFilteringOperation search, Occur occurrence)
        {
            _search = search;
            _occurrence = occurrence;
        }

        /// <inheritdoc/>
        public IBooleanFilterOperation ChainFilters(Action<IFilterChain> chain) => _search.ChainFiltersInternal(chain, _occurrence);

        /// <inheritdoc/>
        public IBooleanFilterOperation TermFilter(FilterTerm term) => _search.TermFilterInternal(term,_occurrence);

        /// <inheritdoc/>
        public IBooleanFilterOperation TermsFilter(IEnumerable<FilterTerm> terms) => _search.TermsFilterInternal(terms, _occurrence);

        /// <inheritdoc/>
        public IBooleanFilterOperation TermPrefixFilter(FilterTerm term) => _search.TermPrefixFilterInternal(term, _occurrence);

        /// <inheritdoc/>
        public IBooleanFilterOperation FieldValueExistsFilter(string field) => _search.FieldValueExistsFilterInternal(field, _occurrence);

        /// <inheritdoc/>
        public IBooleanFilterOperation FieldValueNotExistsFilter(string field) => _search.FieldValueNotExistsFilterInternal(field, _occurrence);

        /// <inheritdoc/>
        public IBooleanFilterOperation QueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And) => _search.QueryFilterInternal(inner, defaultOp, _occurrence);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedChainFilters(Action<IFilterChain> chain) => _search.NestedChainFiltersInternal(chain, _occurrence);

        /// <inheritdoc/>
        public INestedBooleanFilterOperation NestedTermFilter(FilterTerm term) => _search.NestedTermFilterInternal(term, _occurrence);

        /// <inheritdoc/>
        public INestedBooleanFilterOperation NestedTermsFilter(IEnumerable<FilterTerm> terms) => _search.NestedTermsFilterInternal(terms, _occurrence);

        /// <inheritdoc/>
        public INestedBooleanFilterOperation NestedTermPrefix(FilterTerm term) => _search.NestedTermPrefixFilterInternal(term, _occurrence);

        /// <inheritdoc/>
        public INestedBooleanFilterOperation NestedFieldValueExists(string field) => _search.NestedFieldValueExistsFilterInternal(field, _occurrence);

        /// <inheritdoc/>
        public INestedBooleanFilterOperation NestedFieldValueNotExists(string field) => _search.NestedFieldValueNotExistsFilterInternal(field, _occurrence);

        /// <inheritdoc/>
        public INestedBooleanFilterOperation NestedQueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And) => _search.NestedQueryFilterInternal(inner, defaultOp, _occurrence);

        /// <inheritdoc/>
        public IBooleanFilterOperation IntRangeFilter(string field, int? min, int? max, bool minInclusive, bool maxInclusive) => _search.IntRangeFilterInternal(field, min, max, minInclusive, maxInclusive, _occurrence);

        /// <inheritdoc/>
        public IBooleanFilterOperation LongRangeFilter(string field, long? min, long? max, bool minInclusive, bool maxInclusive) => _search.LongRangeFilterInternal(field, min, max, minInclusive, maxInclusive, _occurrence);

        /// <inheritdoc/>
        public IBooleanFilterOperation FloatRangeFilter(string field, float? min, float? max, bool minInclusive, bool maxInclusive) => _search.FloatRangeFilterInternal(field, min, max, minInclusive, maxInclusive, _occurrence);

        /// <inheritdoc/>
        public IBooleanFilterOperation DoubleRangeFilter(string field, double? min, double? max, bool minInclusive, bool maxInclusive) => _search.DoubleRangeFilterInternal(field, min, max, minInclusive, maxInclusive, _occurrence);

        public IBooleanFilterOperation SpatialOperationFilter(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IExamineSpatialShape> shape)
           => _search.SpatialOperationFilterInternal(field, spatialOperation, shape, _occurrence);
    }
}
