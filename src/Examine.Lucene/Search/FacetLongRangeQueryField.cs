using Examine.Search;

namespace Examine.Lucene.Search
{
    public class FacetLongRangeQueryField : LuceneBooleanOperation, IFacetLongRangeQueryField
    {
        public FacetLongRangeQueryField(LuceneSearchQuery search, FacetLongField _) : base(search)
        {
        }
    }
}
