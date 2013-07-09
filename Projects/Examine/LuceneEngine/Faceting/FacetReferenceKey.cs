using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Faceting
{
    public class FacetReferenceKey : FacetKey
    {
        public long ReferenceId { get; private set; }

        public FacetReferenceKey(string fieldName, long referenceId) : base(fieldName, ""+referenceId)
        {
            ReferenceId = referenceId;
        }        
    }
}
