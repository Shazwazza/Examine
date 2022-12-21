using Examine.Lucene.Search;

namespace Examine.Search
{
    public class FacetQueryField : IFacetQueryField
    {
        private readonly LuceneSearchQuery _search;
        private readonly FacetFullTextField _field;

        public FacetQueryField(LuceneSearchQuery search, FacetFullTextField field)
        {
            _search = search;
            _field = field;
        }

        public IFaceting And() => new LuceneFacetOperation(_search);
        public ISearchResults Execute(QueryOptions options = null) => _search.Execute(options);

        public IFacetQueryField FacetField(string fieldName)
        {
            _field.FacetField = fieldName;

            return this;
        }

        public IFacetQueryField MaxCount(int count)
        {
            _field.MaxCount = count;

            return this;
        }
    }
}
