using System;
using System.Collections.Generic;
using System.Text;
using Examine.Lucene.Search;

namespace Examine.Search
{
    public interface IFacetDoubleField : IFacetField
    {
        DoubleRange[] DoubleRanges { get; }

        bool IsFloat { get; set; }
    }
}
