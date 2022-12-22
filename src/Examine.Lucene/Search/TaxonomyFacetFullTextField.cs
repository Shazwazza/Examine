using Examine.Search;

namespace Examine.Lucene.Search
{
    public class TaxonomyFacetFullTextField : ITaxonomyFacetField
    {
        public int MaxCount { get; internal set; }

        public string[] Values { get; }

        public string Field { get; }

        public string FacetField { get; }

        public TaxonomyFacetFullTextField(string field, string[] values, string facetField, int maxCount = 10)
        {
            Field = field;
            Values = values;
            FacetField = facetField;
            MaxCount = maxCount;
        }
    }
}