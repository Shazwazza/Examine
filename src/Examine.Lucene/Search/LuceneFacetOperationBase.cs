using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Search;

namespace Examine.Lucene.Search
{
    public abstract class LuceneFacetOperationBase : IFaceting
    {
        public abstract ISearchResults Execute(QueryOptions options = null);

        public abstract IFacetQueryField WithFacet(string field);

        public abstract IFacetQueryField WithFacet(string field, params string[] values);

        public abstract IFacetDoubleRangeQueryField WithFacet(string field, params DoubleRange[] doubleRanges);

        public abstract IFacetFloatRangeQueryField WithFacet(string field, params FloatRange[] floatRanges);

        public abstract IFacetLongRangeQueryField WithFacet(string field, params Int64Range[] longRanges);
    }
}
