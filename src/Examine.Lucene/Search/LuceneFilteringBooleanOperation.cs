using System;
using System.Collections.Generic;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Filter Boolean Operation
    /// </summary>
    public class LuceneFilteringBooleanOperation : LuceneFilteringBooleanOperationBase
    {
        private readonly LuceneSearchFilteringOperation _search;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="luceneSearch"></param>
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
        public override IBooleanFilterOperation ChainFilters(Action<IFilterChain> chain) => _search.ChainFilters(chain);

        /// <inheritdoc/>
        public override IBooleanFilterOperation TermFilter(FilterTerm term) => _search.TermFilter(term);

        /// <inheritdoc/>
        public override IBooleanFilterOperation TermsFilter(IEnumerable<FilterTerm> terms) => _search.TermsFilter(terms);

        /// <inheritdoc/>
        public override IBooleanFilterOperation TermPrefixFilter(FilterTerm term) => _search.TermPrefixFilter(term);

        /// <inheritdoc/>
        public override IBooleanFilterOperation FieldValueExistsFilter(string field) => _search.FieldValueExistsFilter(field);

        /// <inheritdoc/>
        public override IBooleanFilterOperation FieldValueNotExistsFilter(string field) => _search.FieldValueNotExistsFilter(field);

        /// <inheritdoc/>
        public override IBooleanFilterOperation QueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And) => _search.QueryFilter(inner, defaultOp);

        #endregion

        #region INestedBooleanFilterOperation

        /// <inheritdoc/>
        public override INestedFilter And() =>  new LuceneFilter(_search, Occur.MUST);

        /// <inheritdoc/>
        public override INestedFilter Or() => new LuceneFilter(_search, Occur.SHOULD);

        /// <inheritdoc/>
        public override INestedFilter Not() => new LuceneFilter(_search, Occur.MUST_NOT);

        /// <inheritdoc/>
        public override IBooleanFilterOperation IntRangeFilter(string field, int? min, int? max, bool minInclusive, bool maxInclusive) => _search.IntRangeFilter(field, min, max, minInclusive, maxInclusive);

        /// <inheritdoc/>
        public override IBooleanFilterOperation LongRangeFilter(string field, long? min, long? max, bool minInclusive, bool maxInclusive) => _search.LongRangeFilter(field, min, max, minInclusive, maxInclusive);

        /// <inheritdoc/>
        public override IBooleanFilterOperation FloatRangeFilter(string field, float? min, float? max, bool minInclusive, bool maxInclusive) => _search.FloatRangeFilter(field, min, max, minInclusive, maxInclusive);

        /// <inheritdoc/>
        public override IBooleanFilterOperation DoubleRangeFilter(string field, double? min, double? max, bool minInclusive, bool maxInclusive) => _search.DoubleRangeFilter(field, min, max, minInclusive, maxInclusive);

        #endregion
    }
}
