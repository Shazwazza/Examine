namespace Examine.Search
{
    public readonly struct FacetLongField : IFacetLongField
    {
        public string Field { get; }

        public Int64Range[] LongRanges { get; }

        public string FacetField { get; }

        public FacetLongField(string field, Int64Range[] longRanges, string facetField)
        {
            Field = field;
            LongRanges = longRanges;
            FacetField = facetField;
        }
    }
}
