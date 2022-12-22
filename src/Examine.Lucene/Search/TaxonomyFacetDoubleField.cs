using Examine.Search;

namespace Examine.Lucene.Search
{
    public readonly struct TaxonomyFacetDoubleField : ITaxonomyFacetField
    {
        public DoubleRange[] DoubleRanges { get; }

        public string Field { get; }

        public string FacetField { get; }

        public TaxonomyFacetDoubleField(string field, DoubleRange[] doubleRanges, string facetField)
        {
            Field = field;
            DoubleRanges = doubleRanges;
            FacetField = facetField;
        }
    }
}
