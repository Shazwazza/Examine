using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Examine.Search
{
    public class FacetResult : IFacetResult
    {
        private readonly IEnumerable<IFacetValue> _values;

        public FacetResult(IEnumerable<IFacetValue> values)
        {
            _values = values;
        }

        public IEnumerator<IFacetValue> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public IFacetValue Facet(string label)
        {
            return _values.FirstOrDefault(field => field.Label.Equals(label));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
