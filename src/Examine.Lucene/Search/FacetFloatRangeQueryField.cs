using Examine.Search;

namespace Examine.Lucene.Search
{
    public class FacetFloatRangeQueryField : IFacetFloatRangeQueryField
    {
        private readonly LuceneSearchQuery _search;
        private readonly FacetFloatField _field;

        public FacetFloatRangeQueryField(LuceneSearchQuery search, FacetFloatField field)
        {
            _search = search;
            _field = field;
        }

        public IFaceting And() => new LuceneFacetOperation(_search);
        public ISearchResults Execute(QueryOptions options = null) => _search.Execute(options);

        /// <inheritdoc/>
        public IFacetFloatRangeQueryField FacetField(string fieldName)
        {
            _field.FacetField = fieldName;

            return this;
        }
    }
}
