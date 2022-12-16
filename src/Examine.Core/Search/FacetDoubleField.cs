using System;
using System.Collections.Generic;
using System.Text;
using Examine.Lucene.Search;

namespace Examine.Search
{
    public class FacetDoubleField : IFacetDoubleField
    {
        public DoubleRange[] DoubleRanges { get; }

        public string Field { get; }

        public string FacetField { get; set; }

        public bool IsFloat { get; set; }

        public FacetDoubleField(string field, DoubleRange[] doubleRanges, string facetField = ExamineFieldNames.DefaultFacetsName)
        {
            Field = field;
            DoubleRanges = doubleRanges;
            FacetField = facetField;
        }
    }
}
