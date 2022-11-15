using Examine.Search;

namespace Examine.Lucene.Search
{
    public class FacetDoubleRangeQueryField : LuceneBooleanOperation, IFacetDoubleRangeQueryField
    {
        private readonly FacetDoubleField _field;

        public FacetDoubleRangeQueryField(LuceneSearchQuery search, FacetDoubleField field) : base(search)
        {
            _field = field;
        }

        public IFacetDoubleRangeQueryField IsFloat(bool isFloat)
        {
            _field.IsFloat = isFloat;

            return this;
        }
    }
}
