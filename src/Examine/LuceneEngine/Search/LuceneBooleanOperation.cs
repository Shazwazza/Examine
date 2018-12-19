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
    public class LuceneBooleanOperation : LuceneBooleanOperationBase, IBooleanOperation
    {
        private readonly LuceneSearchQuery _search;
        
        public LuceneBooleanOperation(LuceneSearchQuery search)
            : base(search)
        {
            this._search = search;
        }

        #region IBooleanOperation Members

        /// <inheritdoc />
        public override IQuery And()
        {
            return new LuceneQuery(this._search, Occur.MUST);
        }

        

        /// <inheritdoc />
        public override IQuery Or()
        {
            return new LuceneQuery(this._search, Occur.SHOULD);
        }

        

        /// <inheritdoc />
        public override IQuery Not()
        {
            return new LuceneQuery(this._search, Occur.MUST_NOT);
        }

        
        
        #endregion

        public override ISearchResults Execute(int maxResults = 500)
        {
            return _search.Execute(maxResults);
        }

        #region IOrdering

        public override IOrdering OrderBy(params SortableField[] fields)
        {
            return _search.OrderBy(fields);
        }

        public override IOrdering OrderByDescending(params SortableField[] fields)
        {
            return _search.OrderByDescending(fields);
        }

        #endregion

        

        public override string ToString()
        {
            return _search.ToString();
        }
    }
}
