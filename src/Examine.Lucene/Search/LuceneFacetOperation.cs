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
    public class LuceneFacetOperation : IFaceting
    {
        private readonly LuceneSearchQuery _search;

        public LuceneFacetOperation(LuceneSearchQuery search)
        {
            _search = search;
        }

        public ISearchResults Execute(QueryOptions options = null) => _search.Execute(options);

        public IFaceting Facet(string field, Action<IFacetQueryField> facetConfiguration = null) => _search.FacetInternal(field, facetConfiguration, Array.Empty<string>());

        public IFaceting Facet(string field, Action<IFacetQueryField> facetConfiguration = null, params string[] values) => _search.FacetInternal(field, facetConfiguration, values);

        public IFaceting Facet(string field, params DoubleRange[] doubleRanges) => _search.FacetInternal(field, doubleRanges);

        public IFaceting Facet(string field, params FloatRange[] floatRanges) => _search.FacetInternal(field, floatRanges);

        public IFaceting Facet(string field, params Int64Range[] longRanges) => _search.FacetInternal(field, longRanges);

        public override string ToString() => _search.ToString();
    }
}
