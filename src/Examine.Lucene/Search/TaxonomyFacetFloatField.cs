using Examine.Search;

namespace Examine.Lucene.Search
{
    public readonly struct TaxonomyFacetFloatField : ITaxonomyFacetField
    {
        public FloatRange[] FloatRanges { get; }

        public string Field { get; }

        public string FacetField { get; }

        public TaxonomyFacetFloatField(string field, FloatRange[] floatRanges, string facetField)
        {
            Field = field;
            FloatRanges = floatRanges;
            FacetField = facetField;
        }
    }
}
