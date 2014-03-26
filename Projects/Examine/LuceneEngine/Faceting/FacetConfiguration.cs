using System.Collections.Generic;
using System.Security;

namespace Examine.LuceneEngine.Faceting
{
    public class FacetConfiguration
    {
        private static readonly FacetMap SharedMap = new FacetMap();
        
        public bool IsEmpty
        {
            get { return FacetExtractors.Count == 0; }
        }

        public List<IFacetExtractor> FacetExtractors { get; set; }

        public List<IFacetComb> FacetCombs { get; set; }

        public FacetMap FacetMap { get; set; }

        public IExternalDataProvider ExternalDataProvider { get; set; }

        public bool CacheAllQueryFilters { get; set; }

        public FacetConfiguration()
        {
            FacetMap = SharedMap;
            FacetExtractors = new List<IFacetExtractor>();
            FacetCombs = new List<IFacetComb>() { new MaxLevelFacetComb() };
        }
    }
}
