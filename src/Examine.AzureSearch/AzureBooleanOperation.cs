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
        public override IQuery And()
        {
            return new AzureQuery(_search, BooleanOperation.And);
        }
        
        /// <inheritdoc />
        public override IQuery Or()
        {
            return new AzureQuery(_search, BooleanOperation.Or);
        }

        /// <inheritdoc />
        public override IQuery Not()
        {
            return new AzureQuery(_search, BooleanOperation.Not);
        }

        public override ISearchResults Execute(int maxResults = 500)
        {
            return _search.Execute(maxResults);
        }

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