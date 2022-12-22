using Examine.Search;

namespace Examine.Lucene.Search
{
    public readonly struct TaxonomyFacetLongField : ITaxonomyFacetField
    {
        public string Field { get; }

        public Int64Range[] LongRanges { get; }

        public string FacetField { get; }

        public TaxonomyFacetLongField(string field, Int64Range[] longRanges, string facetField)
        {
            Field = field;
            LongRanges = longRanges;
            FacetField = facetField;
        }
    }
}
