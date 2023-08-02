using System;
using System.Collections.Generic;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    public class LuceneFilteringBooleanOperation : LuceneFilteringBooleanOperationBase
    {
        private readonly LuceneSearchFilteringOperation _search;

        public LuceneFilteringBooleanOperation(LuceneSearchFilteringOperation luceneSearch) : base(luceneSearch)
        {
            _search = luceneSearch;
        }



        #region IBooleanFilterOperation

        /// <inheritdoc/>
        public override IFilter AndFilter() => new LuceneFilter(this._search, Occur.MUST);

        /// <inheritdoc/>
        public override IFilter OrFilter() => new LuceneFilter(this._search, Occur.SHOULD);

        /// <inheritdoc/>
        public override IFilter NotFilter() => new LuceneFilter(this._search, Occur.MUST_NOT);
        #endregion

        #region IFilter

        /// <inheritdoc/>
        public override IBooleanFilterOperation ChainFilters(Action<IFilterChainStart> chain)
        {
            return _search.ChainFilters(chain);
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation TermFilter(FilterTerm term)
        {
            return _search.TermFilter(term);
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation TermsFilter(IEnumerable<FilterTerm> terms)
        {
            return _search.TermsFilter(terms);
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation TermPrefixFilter(FilterTerm term)
        {
            return _search.TermPrefixFilter(term);
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation FieldValueExistsFilter(string field)
        {
            return _search.FieldValueExistsFilter(field);
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation FieldValueNotExistsFilter(string field)
        {
            return _search.FieldValueNotExistsFilter(field);
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation QueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return _search.QueryFilter(inner, defaultOp);
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation RangeFilter<T>(string field, T min, T max, bool minInclusive = true, bool maxInclusive = true)
        {
            return _search.RangeFilter<T>(field, min, max, minInclusive, maxInclusive);
        }

        #endregion
    }
}
