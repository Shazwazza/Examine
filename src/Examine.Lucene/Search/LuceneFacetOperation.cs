using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// An implementation of the fluent API boolean operations
    /// </summary>
    [DebuggerDisplay("{_search}")]
    public class LuceneFacetOperation : LuceneFacetOperationBase
    {
        private readonly LuceneSearchQuery _search;

        public LuceneFacetOperation(LuceneSearchQuery search)
        {
            _search = search;
        }

        public override ISearchResults Execute(QueryOptions options = null) => _search.Execute(options);

        public override IFacetQueryField WithFacet(string field) => _search.FacetInternal(field, Array.Empty<string>());

        public override IFacetQueryField WithFacet(string field, params string[] values) => _search.FacetInternal(field, values);

        public override IFacetDoubleRangeQueryField WithFacet(string field, params DoubleRange[] doubleRanges) => _search.FacetInternal(field, doubleRanges);

        public override IFacetFloatRangeQueryField WithFacet(string field, params FloatRange[] floatRanges) => _search.FacetInternal(field, floatRanges);

        public override IFacetLongRangeQueryField WithFacet(string field, params Int64Range[] longRanges) => _search.FacetInternal(field, longRanges);

        public override string ToString() => _search.ToString();
    }
}
