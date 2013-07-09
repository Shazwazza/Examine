using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Faceting
{
    public class ReferenceFacetValue : FacetValue
    {
        public ReferenceFacetValue(long id, float level = 0.5f) 
            : base(""+id, level)
        {
        }
    }
}
