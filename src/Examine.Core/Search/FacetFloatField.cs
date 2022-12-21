namespace Examine.Search
{
    public class FacetFloatField : IFacetFloatField
    {
        public FloatRange[] FloatRanges { get; }

        public string Field { get; }

        public string FacetField { get; set; }

        public FacetFloatField(string field, FloatRange[] floatRanges, string facetField = ExamineFieldNames.DefaultFacetsName)
        {
            Field = field;
            FloatRanges = floatRanges;
            FacetField = facetField;
        }
    }
}
