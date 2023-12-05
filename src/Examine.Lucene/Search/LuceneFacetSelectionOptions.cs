using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Lucene.Search
{
    public class LuceneFacetSelectionOptions
    {
        public IList<IFacetField> FacetFields { get; set; } = new List<IFacetField>();

        public bool FacetAllFieldsWithHits { get; set; }

        public int FacetAllFieldsWithHitsMaxCount { get; set; }= 10;
    }
}
