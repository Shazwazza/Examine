using Examine.Search;

namespace Examine.Lucene.Search
{
    public class FacetDoubleRangeQueryField : IFacetDoubleRangeQueryField
    {
        private readonly LuceneSearchQuery _search;
        private readonly FacetDoubleField _field;

        public FacetDoubleRangeQueryField(LuceneSearchQuery search, FacetDoubleField field)
        {
            _search = search;
            _field = field;
        }

        public IOrdering And() => new LuceneBooleanOperation(_search);
        public ISearchResults Execute(QueryOptions options = null) => _search.Execute(options);

        public IFacetDoubleRangeQueryField IsFloat(bool isFloat)
        {
            _field.IsFloat = isFloat;

            return this;
        }
    }
}
