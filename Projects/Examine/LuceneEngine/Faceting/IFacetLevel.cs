using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Faceting
{
    public interface IFacetLevel
    {
        FacetLevel ToFacetLevel(ISearcherContext context);
    }
}
