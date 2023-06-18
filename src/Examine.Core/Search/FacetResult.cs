using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Examine.Search
{
    /// <inheritdoc/>
    public class FacetResult : IFacetResult
    {
        private readonly IEnumerable<IFacetValue> _values;
        private IDictionary<string, IFacetValue> _dictValues;

        /// <inheritdoc/>
        public FacetResult(IEnumerable<IFacetValue> values)
        {
            _values = values;
        }

        /// <inheritdoc/>
        public IEnumerator<IFacetValue> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        private void SetValuesDictionary()
        {
            if(_dictValues == null)
            {
                _dictValues = _values.ToDictionary(src => src.Label, src => src);
            }
        }

        /// <inheritdoc/>
        public IFacetValue Facet(string label)
        {
            SetValuesDictionary();
            return _dictValues[label];
        }

        /// <inheritdoc/>
        public bool TryGetFacet(string label, out IFacetValue facetValue)
        {
            SetValuesDictionary();
            return _dictValues.TryGetValue(label, out facetValue);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
