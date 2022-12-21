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

        public IFaceting And() => new LuceneFacetOperation(_search);
        public ISearchResults Execute(QueryOptions options = null) => _search.Execute(options);

        /// <inheritdoc/>
        public IFacetDoubleRangeQueryField FacetField(string fieldName)
        {
            _field.FacetField = fieldName;

            return this;
        }
    }
}
