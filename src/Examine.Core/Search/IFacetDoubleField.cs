using System;
using System.Collections.Generic;
using System.Text;
using Examine.Lucene.Search;
using Lucene.Net.Facet.Range;

namespace Examine.Search
{
    public interface IFacetDoubleField : IFacetField
    {
        DoubleRange[] DoubleRanges { get; }

        bool IsFloat { get; set; }
    }
}
