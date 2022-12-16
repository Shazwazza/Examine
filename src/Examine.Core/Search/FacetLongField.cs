namespace Examine.Search
{
    public class FacetLongField : IFacetLongField
    {
        public string Field { get; }

        public Int64Range[] LongRanges { get; }

        public string FacetField { get; set; }

        public FacetLongField(string field, Int64Range[] longRanges, string facetField = ExamineFieldNames.DefaultFacetsName)
        {
            Field = field;
            LongRanges = longRanges;
            FacetField = facetField;
        }
    }
}
