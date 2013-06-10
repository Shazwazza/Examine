using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Faceting
{
    public class ReferenceFacetField : FacetField
    {
        public ReferenceFacetField(string name, long id, float level = 0.5f, bool store = false) : base(name, ""+id, level, store)
        {
        }
    }
}
