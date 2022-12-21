namespace Examine.Search
{
    public class FacetFullTextField : IFacetFullTextField
    {
        public int MaxCount { get; set; }

        public string[] Values { get; }

        public string Field { get; }

        public string FacetField { get; }

        public FacetFullTextField(string field, string[] values, string facetField, int maxCount = 10)
        {
            Field = field;
            Values = values;
            FacetField = facetField;
            MaxCount = maxCount;
        }
    }
}
