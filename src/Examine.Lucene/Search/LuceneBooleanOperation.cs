using System;
using System.Collections.Generic;
using System.Diagnostics;
using Examine.Lucene.Providers;
using Examine.Search;
using Lucene.Net.Facet.Range;
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

        public override ISearchResults Execute(QueryOptions options = null) => _search.Execute(options);

        #region IOrdering

        public override IOrdering OrderBy(params SortableField[] fields) => _search.OrderBy(fields);

        public override IOrdering OrderByDescending(params SortableField[] fields) => _search.OrderByDescending(fields);

        #endregion

        #region Select Fields

        public override IOrdering SelectFields(ISet<string> fieldNames) => _search.SelectFieldsInternal(fieldNames);

        public override IOrdering SelectField(string fieldName) => _search.SelectFieldInternal(fieldName);

        public override IOrdering SelectAllFields() => _search.SelectAllFieldsInternal();

        #endregion

        public override string ToString() => _search.ToString();

        public override IFacetQueryField WithFacet(string field) => _search.FacetInternal(field, Array.Empty<string>());

        public override IFacetQueryField WithFacet(string field, params string[] values) => _search.FacetInternal(field, values);

        public override IFacetDoubleRangeQueryField WithFacet(string field, params DoubleRange[] doubleRanges) => _search.FacetInternal(field, doubleRanges);

        public override IFacetLongRangeQueryField WithFacet(string field, params Int64Range[] longRanges) => _search.FacetInternal(field, longRanges);
    }
}
