using System;
using System.Diagnostics;
using Examine.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// An implementation of the fluent API boolean operations
    /// </summary>
    [DebuggerDisplay("{_search}")]
    public class LuceneFacetOperation : IFacetOperations
    {
        private readonly LuceneSearchQuery _search;

        /// <inheritdoc/>
        public LuceneFacetOperation(LuceneSearchQuery search)
        {
            _search = search;
        }

        /// <inheritdoc/>
        public ISearchResults Execute(QueryOptions? options = null) => _search.Execute(options);

        /// <inheritdoc/>
        public IFacetOperations FacetString(string field, Action<IFacetQueryField>? facetConfiguration = null, params string[] values) => _search.FacetInternal(field, facetConfiguration, values);

        /// <inheritdoc/>
        public IFacetOperations FacetDoubleRange(string field, params DoubleRange[] doubleRanges) => _search.FacetInternal(field, doubleRanges);

        /// <inheritdoc/>
        public IFacetOperations FacetFloatRange(string field, params FloatRange[] floatRanges) => _search.FacetInternal(field, floatRanges);

        /// <inheritdoc/>
        public IFacetOperations FacetLongRange(string field, params Int64Range[] longRanges) => _search.FacetInternal(field, longRanges);

        /// <inheritdoc/>
        public override string ToString() => _search.ToString();
    }
}
