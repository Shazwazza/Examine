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

        public override bool Equals(FacetKey other)
        {
            if (ReferenceEquals(this, other)) return true;
            
            var otherKey = other as FacetReferenceKey;
            if (otherKey != null)
            {
                return otherKey.ReferenceId == ReferenceId;
            }

            return base.Equals(other);
        }
    }
}
