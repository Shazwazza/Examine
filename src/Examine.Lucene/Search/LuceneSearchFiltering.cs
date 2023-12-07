using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Search;
using Lucene.Net.Index;
using Lucene.Net.Queries;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    // LuceneSearchQuery
    public class LuceneSearchFilteringOperation : LuceneSearchFilteringOperationBase
    {
        private readonly LuceneSearchQuery _luceneSearchQuery;

        public LuceneSearchFilteringOperation(LuceneSearchQuery luceneSearchQuery)
           : base(luceneSearchQuery)
        {
            _luceneSearchQuery = luceneSearchQuery;
        }

        /// <summary>
        /// Creates a new <see cref="LuceneFilteringBooleanOperation"/>
        /// </summary>
        /// <returns></returns>
        protected override LuceneFilteringBooleanOperationBase CreateBooleanOp() => new LuceneFilteringBooleanOperation(this);

        /// <summary>
        /// Creates a new <see cref="FilterChainOpBase"/>
        /// </summary>
        /// <returns></returns>
        protected override FilterChainOpBase CreateChainOp() => new FilterChainOp(this);
        #region IFilter

        /// <inheritdoc/>
        public override IBooleanFilterOperation ChainFilters(Action<IFilterChainStart> chain)
        {
            return ChainFiltersInternal(chain);
        }

        private IBooleanFilterOperation ChainFiltersInternal(Action<IFilterChainStart> chain, Occur occurance = Occur.MUST)
        {
            if (chain is null)
            {
                throw new ArgumentNullException(nameof(chain));
            }
            var chaining = CreateChainOp();
            chain.Invoke(chaining);
            var chainedFilter = chaining.Build();
            if (chainedFilter != null)
            {
                Filter.Add(chainedFilter, occurance);
            }

            return CreateBooleanOp();
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation TermFilter(FilterTerm term) => TermFilterInternal(term);

        /// <inheritdoc/>
        internal IBooleanFilterOperation TermFilterInternal(FilterTerm term, Occur occurance = Occur.MUST)
        {
            if (term.FieldName is null)
            {
                throw new ArgumentNullException(nameof(term.FieldName));
            }

            var filterToAdd = new TermFilter(new Term(term.FieldName, term.FieldValue));
            if (filterToAdd != null)
            {
                Filter.Add(filterToAdd, occurance);
            }

            return CreateBooleanOp();
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation TermsFilter(IEnumerable<FilterTerm> terms) => TermsInternal(terms);

        /// <inheritdoc/>
        internal IBooleanFilterOperation TermsInternal(IEnumerable<FilterTerm> terms, Occur occurance = Occur.MUST)
        {
            if (terms is null)
            {
                throw new ArgumentNullException(nameof(terms));
            }

            if (!terms.Any() || terms.Any(x => string.IsNullOrWhiteSpace(x.FieldName)))
            {
                throw new ArgumentOutOfRangeException(nameof(terms));
            }

            var luceneTerms = terms.Select(x => new Term(x.FieldName, x.FieldValue)).ToArray();
            var filterToAdd = new TermsFilter(luceneTerms);
            if (filterToAdd != null)
            {
                Filter.Add(filterToAdd, occurance);
            }

            return CreateBooleanOp();
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation TermPrefixFilter(FilterTerm term) => TermPrefixFilterInternal(term);

        /// <inheritdoc/>
        internal IBooleanFilterOperation TermPrefixFilterInternal(FilterTerm term, Occur occurance = Occur.MUST)
        {
            if (term.FieldName is null)
            {
                throw new ArgumentNullException(nameof(term.FieldName));
            }

            var filterToAdd = new PrefixFilter(new Term(term.FieldName, term.FieldValue));
            if (filterToAdd != null)
            {
                Filter.Add(filterToAdd, occurance);
            }

            return CreateBooleanOp();
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation FieldValueExistsFilter(string field) => FieldValueExistsFilterInternal(field);

        /// <inheritdoc/>
        internal IBooleanFilterOperation FieldValueExistsFilterInternal(string field, Occur occurance = Occur.MUST)
        {
            if (field is null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            var filterToAdd = new FieldValueFilter(field);
            if (filterToAdd != null)
            {
                Filter.Add(filterToAdd, occurance);
            }

            return CreateBooleanOp();
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation FieldValueNotExistsFilter(string field) => FieldValueNotExistsFilterInternal(field);

        /// <inheritdoc/>
        internal IBooleanFilterOperation FieldValueNotExistsFilterInternal(string field, Occur occurance = Occur.MUST)
        {
            if (field is null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            var filterToAdd = new FieldValueFilter(field);
            if (filterToAdd != null)
            {
                Filter.Add(filterToAdd, occurance);
            }

            return CreateBooleanOp();
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation QueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And) => QueryFilterInternal(inner, defaultOp);

        /// <inheritdoc/>
        internal IBooleanFilterOperation QueryFilterInternal(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp, Occur occurance = Occur.MUST)
        {
            if (inner is null)
            {
                throw new ArgumentNullException(nameof(inner));
            }

            Func<Query, Filter> buildFilter = (baseQuery) =>
            {
                var queryWrapperFilter = new QueryWrapperFilter(baseQuery);

                return queryWrapperFilter;
            };

            var bo = new LuceneBooleanOperation(_luceneSearchQuery);

            var baseOp = bo.OpBaseFilter(buildFilter, inner, occurance.ToBooleanOperation(), defaultOp);

            var op = CreateBooleanOp();
            return op;
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation DoubleRangeFilter(string field, double? min, double? max, bool minInclusive, bool maxInclusive)
        {
            return DoubleRangeFilterInternal(field, min, max, minInclusive, maxInclusive);
        }

        internal IBooleanFilterOperation DoubleRangeFilterInternal(string field, double? min, double? max, bool minInclusive, bool maxInclusive, Occur occurance = Occur.MUST)
        {
            if (field is null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            var filterToAdd = NumericRangeFilter.NewDoubleRange(field, min, max, minInclusive, maxInclusive);
            if (filterToAdd != null)
            {
                Filter.Add(filterToAdd, occurance);
            }

            return CreateBooleanOp();
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation FloatRangeFilter(string field, float? min, float? max, bool minInclusive, bool maxInclusive)
        {
            return FloatRangeFilterInternal(field, min, max, minInclusive, maxInclusive);
        }

        internal IBooleanFilterOperation FloatRangeFilterInternal(string field, float? min, float? max, bool minInclusive, bool maxInclusive, Occur occurance = Occur.MUST)
        {
            if (field is null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            var filterToAdd = NumericRangeFilter.NewSingleRange(field, min, max, minInclusive, maxInclusive);
            if (filterToAdd != null)
            {
                Filter.Add(filterToAdd, occurance);
            }

            return CreateBooleanOp();
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation IntRangeFilter(string field, int? min, int? max, bool minInclusive, bool maxInclusive)
        {
            return IntRangeFilterInternal(field, min, max, minInclusive, maxInclusive);
        }

        internal IBooleanFilterOperation IntRangeFilterInternal(string field, int? min, int? max, bool minInclusive, bool maxInclusive, Occur occurance = Occur.MUST)
        {
            if (field is null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            var filterToAdd = NumericRangeFilter.NewInt32Range(field, min, max, minInclusive, maxInclusive);
            if (filterToAdd != null)
            {
                Filter.Add(filterToAdd, occurance);
            }

            return CreateBooleanOp();
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation LongRangeFilter(string field, long? min, long? max, bool minInclusive, bool maxInclusive)
        {
            return LongRangeFilterInternal(field, min, max, minInclusive, maxInclusive);
        }

        internal IBooleanFilterOperation LongRangeFilterInternal(string field, long? min, long? max, bool minInclusive, bool maxInclusive, Occur occurance = Occur.MUST)
        {
            if (field is null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            var filterToAdd = NumericRangeFilter.NewInt64Range(field, min, max, minInclusive, maxInclusive);
            if (filterToAdd != null)
            {
                Filter.Add(filterToAdd, occurance);
            }

            return CreateBooleanOp();
        }
        #endregion


        #region INestedFilter

        /// <inheritdoc/>
        protected override INestedBooleanFilterOperation NestedChainFilters(Action<IFilterChainStart> chain) => NestedChainFiltersInternal(chain);

        /// <inheritdoc/>
        protected override INestedBooleanFilterOperation NestedTermFilter(FilterTerm term) => NestedTermFilterInternal(term);

        /// <inheritdoc/>
        protected override INestedBooleanFilterOperation NestedTermsFilter(IEnumerable<FilterTerm> terms) => NestedTermsFilterInternal(terms);

        /// <inheritdoc/>
        protected override INestedBooleanFilterOperation NestedTermPrefixFilter(FilterTerm term) => NestedTermPrefixFilterInternal(term);

        /// <inheritdoc/>
        protected override INestedBooleanFilterOperation NestedFieldValueExistsFilter(string field) => NestedFieldValueExistsFilterInternal(field);

        /// <inheritdoc/>
        protected override INestedBooleanFilterOperation NestedFieldValueNotExistsFilter(string field) => NestedFieldValueNotExistsFilterInternal(field);

        /// <inheritdoc/>
        protected override INestedBooleanFilterOperation NestedQueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp) => NestedQueryFilterInternal(inner, defaultOp);

        /// <inheritdoc/>
        internal INestedBooleanFilterOperation NestedChainFiltersInternal(Action<IFilterChainStart> chain, Occur occurance = Occur.MUST)
        {
            if (chain is null)
            {
                throw new ArgumentNullException(nameof(chain));
            }
            var chaining = new FilterChainOp(this);
            chain.Invoke(chaining);
            var chainedFilter = chaining.Build();
            if (chainedFilter != null)
            {
                Filter.Add(chainedFilter, occurance);
            }

            return CreateBooleanOp();
        }

        /// <inheritdoc/>
        internal INestedBooleanFilterOperation NestedTermFilterInternal(FilterTerm term, Occur occurance = Occur.MUST)
        {
            if (term.FieldName is null)
            {
                throw new ArgumentNullException(nameof(term.FieldName));
            }

            var filterToAdd = new TermFilter(new Term(term.FieldName, term.FieldValue));
            if (filterToAdd != null)
            {
                Filter.Add(filterToAdd, occurance);
            }

            return CreateBooleanOp();
        }

        /// <inheritdoc/>
        internal INestedBooleanFilterOperation NestedTermsFilterInternal(IEnumerable<FilterTerm> terms, Occur occurance = Occur.MUST)
        {
            if (terms is null)
            {
                throw new ArgumentNullException(nameof(terms));
            }

            if (!terms.Any() || terms.Any(x => string.IsNullOrWhiteSpace(x.FieldName)))
            {
                throw new ArgumentOutOfRangeException(nameof(terms));
            }

            var luceneTerms = terms.Select(x => new Term(x.FieldName, x.FieldValue)).ToArray();
            var filterToAdd = new TermsFilter(luceneTerms);
            if (filterToAdd != null)
            {
                Filter.Add(filterToAdd, occurance);
            }

            return CreateBooleanOp();
        }

        /// <inheritdoc/>
        internal INestedBooleanFilterOperation NestedTermPrefixFilterInternal(FilterTerm term, Occur occurance = Occur.MUST)
        {
            if (term.FieldName is null)
            {
                throw new ArgumentNullException(nameof(term.FieldName));
            }

            var filterToAdd = new PrefixFilter(new Term(term.FieldName, term.FieldValue));
            if (filterToAdd != null)
            {
                Filter.Add(filterToAdd, occurance);
            }

            return CreateBooleanOp();
        }

        /// <inheritdoc/>
        internal INestedBooleanFilterOperation NestedFieldValueExistsFilterInternal(string field, Occur occurance = Occur.MUST)
        {
            if (field is null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            var filterToAdd = new FieldValueFilter(field);
            if (filterToAdd != null)
            {
                Filter.Add(filterToAdd, occurance);
            }

            return CreateBooleanOp();
        }

        /// <inheritdoc/>
        internal INestedBooleanFilterOperation NestedFieldValueNotExistsFilterInternal(string field, Occur occurance = Occur.MUST)
        {
            if (field is null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            var filterToAdd = new FieldValueFilter(field);
            if (filterToAdd != null)
            {
                Filter.Add(filterToAdd, occurance);
            }

            return CreateBooleanOp();
        }

        /// <inheritdoc/>
        internal INestedBooleanFilterOperation NestedQueryFilterInternal(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp, Occur occurance = Occur.MUST)
        {
            if (inner is null)
            {
                throw new ArgumentNullException(nameof(inner));
            }

            Func<Query, Filter> buildFilter = (baseQuery) =>
            {
                var queryWrapperFilter = new QueryWrapperFilter(baseQuery);

                return queryWrapperFilter;
            };

            var bo = new LuceneBooleanOperation(_luceneSearchQuery);

            var baseOp = bo.OpBaseFilter(buildFilter, inner, occurance.ToBooleanOperation(), defaultOp);

            var op = CreateBooleanOp();
            return op;
        }

        #endregion

    }
}
