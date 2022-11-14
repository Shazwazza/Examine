using Examine.Lucene.Search;

namespace Examine.Search
{
    public class FacetQueryField : LuceneBooleanOperation, IFacetQueryField
    {
        private readonly FacetFullTextField _field;

        public FacetQueryField(LuceneSearchQuery search, FacetFullTextField field) : base(search)
        {
            _field = field;
        }

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
