namespace Examine.Search
{
    public class FacetFullTextField : IFacetField
    {
        public int MaxCount { get; internal set; }

        public string[] Values { get; }

        public string Field { get; }

        public string FacetField { get; }

        public string[] Path { get; internal set; }

        public FacetFullTextField(string field, string[] values, string facetField, int maxCount = 10)
        {
            Field = field;
            Values = values;
            FacetField = facetField;
            MaxCount = maxCount;
        }
    }
}
