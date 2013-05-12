using System.Collections.Generic;

namespace Examine.LuceneEngine.Facets
{
    public interface IFacetGraph
    {
        IEnumerable<FacetKey> GetParents(FacetKey key);
    }


}
