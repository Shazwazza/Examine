namespace Examine.Search
{
    public class FacetFullTextField : IFacetFullTextField
    {
        public int MaxCount { get; set; }

        public string[] Values { get; }

        public string Field { get; }

        public string FacetField { get; set; }

        public FacetFullTextField(string field, string[] values, int maxCount = 10, string facetField = ExamineFieldNames.DefaultFacetsName)
        {
            Field = field;
            Values = values;
            MaxCount = maxCount;
            FacetField = facetField;
        }
    }
}
