using Examine.Search;

namespace Examine.Lucene.Search
{
    public class FacetRangeQueryField<T> : LuceneBooleanOperation, IFacetRangeQueryField
        where T : IFacetField
    {
        private readonly T _field;

        public FacetRangeQueryField(LuceneSearchQuery search, T field) : base(search)
        {
            _field = field;
        }

        public IFacetRangeQueryField FacetField(string fieldName)
        {
            _field.FacetField = fieldName;

            return this;
        }
    }
}
