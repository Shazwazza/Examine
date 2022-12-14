using Examine.Search;

namespace Examine.Lucene.Search
{
    public class FacetLongRangeQueryField : IFacetLongRangeQueryField
    {
        private readonly LuceneSearchQuery _search;

        public FacetLongRangeQueryField(LuceneSearchQuery search, FacetLongField _)
        {
            _search = search;
        }

        public IOrdering And() => new LuceneBooleanOperation(_search);
        public ISearchResults Execute(QueryOptions options = null) => _search.Execute(options);
    }
}
