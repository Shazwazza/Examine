using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Examine.Search;

namespace Examine.Lucene.Search
{
    /// <inheritdoc/>
    internal sealed class FacetResult(IEnumerable<IFacetValue> values) : IFacetResult
    {
        private readonly IEnumerable<IFacetValue> _values = values;

        [AllowNull]
        private Dictionary<string, IFacetValue> _dictValues;

        /// <inheritdoc/>
        public IEnumerator<IFacetValue> GetEnumerator() => _values.GetEnumerator();

        [MemberNotNull(nameof(_dictValues))]
        private void SetValuesDictionary() => _dictValues ??= _values.ToDictionary(src => src.Label, src => src);

        /// <inheritdoc/>
        public IFacetValue? Facet(string label)
        {
            SetValuesDictionary();
            return _dictValues[label];
        }

        /// <inheritdoc/>
        public bool TryGetFacet(string label, out IFacetValue? facetValue)
        {
            SetValuesDictionary();
            return _dictValues.TryGetValue(label, out facetValue);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
