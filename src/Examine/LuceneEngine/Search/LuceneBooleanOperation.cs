using System.Diagnostics;
using Examine.LuceneEngine.Providers;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Search
{
    /// <summary>
    /// An implementation of the fluent API boolean operations
    /// </summary>
    [DebuggerDisplay("{_search}")]
    public class LuceneBooleanOperation : LuceneBooleanOperationBase, IQueryExecutor2
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

        public override ISearchResults Execute(int maxResults = 500) => _search.Execute(maxResults);

        #region IOrdering

        public override IOrdering OrderBy(params SortableField[] fields) => _search.OrderBy(fields);

        public override IOrdering OrderByDescending(params SortableField[] fields) => _search.OrderByDescending(fields);

        #endregion

        public override string ToString() => _search.ToString();

        public override ISearchResults ExecuteWithSkip(int skip, int? take = null) => _search.ExecuteWithSkip(skip, take);
    }
}
