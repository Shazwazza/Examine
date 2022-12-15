using System;
using System.Collections.Generic;
using System.Text;
using Examine.Lucene.Search;
using Lucene.Net.Facet.Range;

namespace Examine.Search
{
    public class FacetDoubleField : IFacetDoubleField
    {
        public DoubleRange[] DoubleRanges { get; }

        public string Field { get; }

        public string FacetField { get; set; }

        public bool IsFloat { get; set; }

        public FacetDoubleField(string field, DoubleRange[] doubleRanges)
        {
            Field = field;
            DoubleRanges = doubleRanges;
            FacetField = ExamineFieldNames.DefaultFacetsName;
        }
    }
}
