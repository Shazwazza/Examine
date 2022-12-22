namespace Examine.Search
{
    public readonly struct FacetFloatField : IFacetField
    {
        public FloatRange[] FloatRanges { get; }

        public string Field { get; }

        public string FacetField { get; }

        public FacetFloatField(string field, FloatRange[] floatRanges, string facetField)
        {
            Field = field;
            FloatRanges = floatRanges;
            FacetField = facetField;
        }
    }
}
