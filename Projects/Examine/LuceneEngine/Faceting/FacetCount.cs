using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Faceting
{
    public struct FacetCount
    {
        public FacetKey Key;

        public int Count;

        public FacetCount(FacetKey key, int count)
        {
            Key = key;
            Count = count;
        }
    }
}
