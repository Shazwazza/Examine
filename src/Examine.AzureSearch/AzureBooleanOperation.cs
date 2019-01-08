using System;
using Examine.LuceneEngine.Search;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.AzureSearch
{
    public class AzureBooleanOperation : LuceneBooleanOperationBase
    {
        private readonly AzureQuery _search;

        internal AzureBooleanOperation(AzureQuery search)
            : base(search)
        {
            _search = search;
        }

        /// <inheritdoc />
        public override IQuery And() => new AzureQuery(_search, BooleanOperation.And);

        /// <inheritdoc />
        public override IQuery Or() => new AzureQuery(_search, BooleanOperation.Or);

        /// <inheritdoc />
        public override IQuery Not() => new AzureQuery(_search, BooleanOperation.Not);

        protected override INestedQuery AndNested() => new AzureQuery(_search, BooleanOperation.And);

        protected override INestedQuery OrNested() => new AzureQuery(_search, BooleanOperation.Or);

        protected override INestedQuery NotNested() => new AzureQuery(_search, BooleanOperation.Not);

        public override ISearchResults Execute(int maxResults = 500) => _search.Execute(maxResults);

        #region IOrdering

        public override IOrdering OrderBy(params SortableField[] fields)
        {
            throw new NotImplementedException();
            //return _search.OrderBy(fields);
        }

        public override IOrdering OrderByDescending(params SortableField[] fields)
        {
            throw new NotImplementedException();
            //return _search.OrderByDescending(fields);
        }

        #endregion

    }
}