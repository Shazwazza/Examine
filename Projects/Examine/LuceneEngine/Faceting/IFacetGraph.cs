using System.Collections.Generic;

namespace Examine.LuceneEngine.Faceting
{
    public interface IFacetGraph
    {
        IEnumerable<FacetKey> GetParents(FacetKey key);
    }


}
