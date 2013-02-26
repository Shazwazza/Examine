using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Faceting
{
    public class FacetConfiguration
    {
        private static FacetMap _sharedMap = new FacetMap();

        public List<IFacetExtractor> FacetExtractors { get; set; }

        public List<IFacetComb> FacetCombs { get; set; }

        public FacetMap FacetMap { get; set; }

        public bool CacheAllQueryFilters { get; set; }

        public FacetConfiguration()
        {
            FacetMap = _sharedMap;
            FacetExtractors = new List<IFacetExtractor>();
            FacetCombs = new List<IFacetComb>() { new MaxLevelFacetComb() };
        }
    }
}
