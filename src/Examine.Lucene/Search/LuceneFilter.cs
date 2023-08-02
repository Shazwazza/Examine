using System;
using System.Collections.Generic;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
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
        public IBooleanFilterOperation ChainFilters(Action<IFilterChainStart> chain) => _search.ChainFilters(chain);

        /// <inheritdoc/>
        public IBooleanFilterOperation TermFilter(FilterTerm term) => _search.TermFilter(term);

        /// <inheritdoc/>
        public IBooleanFilterOperation TermsFilter(IEnumerable<FilterTerm> terms) => _search.TermsFilter(terms);

        /// <inheritdoc/>
        public IBooleanFilterOperation TermPrefixFilter(FilterTerm term) => _search.TermPrefixFilter(term);

        /// <inheritdoc/>
        public IBooleanFilterOperation FieldValueExistsFilter(string field) => _search.FieldValueExistsFilter(field);

        /// <inheritdoc/>
        public IBooleanFilterOperation FieldValueNotExistsFilter(string field) => _search.FieldValueNotExistsFilter(field);

        /// <inheritdoc/>
        public IBooleanFilterOperation QueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And) => _search.QueryFilter(inner, defaultOp);

        /// <inheritdoc/>
        public IBooleanFilterOperation RangeFilter<T>(string field, T min, T max, bool minInclusive = true, bool maxInclusive = true) where T : struct => _search.RangeFilter(field, min, max, minInclusive, maxInclusive);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedChainFilters(Action<IFilterChainStart> chain) => _search.NestedChainFiltersInternal(chain);

        /// <inheritdoc/>
        public INestedBooleanFilterOperation NestedTermFilter(FilterTerm term) => _search.NestedTermFilterInternal(term);

        /// <inheritdoc/>
        public INestedBooleanFilterOperation NestedTermsFilter(IEnumerable<FilterTerm> terms) => _search.NestedTermsFilterInternal(terms);

        /// <inheritdoc/>
        public INestedBooleanFilterOperation NestedTermPrefix(FilterTerm term) => _search.NestedTermPrefixFilterInternal(term);

        /// <inheritdoc/>
        public INestedBooleanFilterOperation NestedFieldValueExists(string field) => _search.NestedFieldValueExistsFilterInternal(field);

        /// <inheritdoc/>
        public INestedBooleanFilterOperation NestedFieldValueNotExists(string field) => _search.NestedFieldValueNotExistsFilterInternal(field);

        /// <inheritdoc/>
        public INestedBooleanFilterOperation NestedQueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And) => _search.NestedQueryFilterInternal(inner, defaultOp);

        /// <inheritdoc/>
        public INestedBooleanFilterOperation NestedRangeFilter<T>(string field, T min, T max, bool minInclusive = true, bool maxInclusive = true) where T : struct => _search.NestedRangeFilterInternal(field, min, max, minInclusive, maxInclusive);
    }
}
