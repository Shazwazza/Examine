using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Faceting
{
    public struct FacetLevel : IFacetLevel
    {
        public int FacetId;

        public float Level;


        
        FacetLevel IFacetLevel.ToFacetLevel(ISearcherContext context)
        {
            return this;
        }
    }
}
