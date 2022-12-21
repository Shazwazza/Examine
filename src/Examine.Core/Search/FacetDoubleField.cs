namespace Examine.Search
{
    public readonly struct FacetDoubleField : IFacetDoubleField
    {
        public DoubleRange[] DoubleRanges { get; }

        public string Field { get; }

        public string FacetField { get; }

        public FacetDoubleField(string field, DoubleRange[] doubleRanges, string facetField)
        {
            Field = field;
            DoubleRanges = doubleRanges;
            FacetField = facetField;
        }
    }
}
