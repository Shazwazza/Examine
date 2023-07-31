using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using Examine.Lucene.Providers;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// An implementation of the fluent API boolean operations
    /// </summary>
    [DebuggerDisplay("{_search}")]
    public class LuceneBooleanOperation : LuceneBooleanOperationBase, IQueryExecutor
    {
        private readonly LuceneSearchQuery _search;

        /// <inheritdoc/>
        public LuceneBooleanOperation(LuceneSearchQuery search)
            : base(search)
        {
            _search = search;
        }

        #region IBooleanOperation Members

        /// <inheritdoc />
        protected override INestedQuery AndNested() => new LuceneQuery(this._search, Occur.MUST);

        /// <inheritdoc />
        protected override INestedQuery OrNested() => new LuceneQuery(this._search, Occur.SHOULD);

        /// <inheritdoc />
        protected override INestedQuery NotNested() => new LuceneQuery(this._search, Occur.MUST_NOT);

        /// <inheritdoc />
        public override IQuery And() => new LuceneQuery(this._search, Occur.MUST);


        /// <inheritdoc />
        public override IQuery Or() => new LuceneQuery(this._search, Occur.SHOULD);


        /// <inheritdoc />
        public override IQuery Not() => new LuceneQuery(this._search, Occur.MUST_NOT);

        #endregion

        /// <inheritdoc/>
        public override ISearchResults Execute(QueryOptions? options = null) => _search.Execute(options);

        #region IOrdering

        /// <inheritdoc/>
        public override IOrdering OrderBy(params SortableField[] fields) => _search.OrderBy(fields);

        /// <inheritdoc/>
        public override IOrdering OrderByDescending(params SortableField[] fields) => _search.OrderByDescending(fields);

        #endregion

        #region Select Fields

        /// <inheritdoc/>
        public override IOrdering SelectFields(ISet<string> fieldNames) => _search.SelectFieldsInternal(fieldNames);

        /// <inheritdoc/>
        public override IOrdering SelectField(string fieldName) => _search.SelectFieldInternal(fieldName);

        /// <inheritdoc/>
        public override IOrdering SelectAllFields() => _search.SelectAllFieldsInternal();

        #endregion

        /// <inheritdoc/>
        public override string ToString() => _search.ToString();

        /// <inheritdoc/>
        public override IQueryExecutor WithFacets(Action<IFacetOperations> facets)
        {
            var luceneFacetOperation = new LuceneFacetOperation(_search);
            facets.Invoke(luceneFacetOperation);
            return luceneFacetOperation;
        }

        #region IFilter

        /// <inheritdoc/>
        public override IBooleanFilterOperation ChainFilters(Action<IFilterChainStart> chain)
        {
            return _search.ChainFilters(chain);
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation Term(FilterTerm term)
        {
            return _search.Term(term);
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation Terms(IEnumerable<FilterTerm> terms)
        {
            return _search.Terms(terms);
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation TermPrefix(FilterTerm term)
        {
            return _search.TermPrefix(term);
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation FieldValueExists(string field)
        {
            return _search.FieldValueExists(field);
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation FieldValueNotExists(string field)
        {
            return _search.FieldValueNotExists(field);
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation QueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And)
        {
            return _search.QueryFilter(inner, defaultOp);
        }

        /// <inheritdoc/>
        public override IBooleanFilterOperation RangeFilter<T>(string field, T min, T max, bool minInclusive = true, bool maxInclusive = true)
        {
            return _search.RangeFilter<T>(field, min, max, minInclusive, maxInclusive );
        }

        #endregion
    }
}
