using System;
using System.Collections.Generic;
using System.Security;
using Examine.LuceneEngine.Config;

namespace Examine.LuceneEngine.Faceting
{
    public class FacetConfiguration
    {
        private static FacetMap _sharedMap = new FacetMap();
        

        public List<IFacetExtractor> FacetExtractors { get; set; }

        public List<IFacetComb> FacetCombs { get; set; }

        public FacetMap FacetMap { get; set; }

        public IExternalDataProvider ExternalDataProvider { get; set; }

        public bool CacheAllQueryFilters { get; set; }

        public FacetConfiguration()
        {
            FacetMap = _sharedMap;
            FacetExtractors = new List<IFacetExtractor>();
            FacetCombs = new List<IFacetComb>() { new MaxLevelFacetComb() };
        }
    }

    internal static class FacetConfigurationHelpers
    {
        [SecuritySafeCritical]
        public static FacetConfiguration GetFacetConfiguration(this IndexSet set)
        {
            var config = new FacetConfiguration();
            foreach (var subset in new[] { set.IndexUserFields, set.IndexAttributeFields })
            {
                if (subset != null)
                {
                    foreach (IIndexField f in subset)
                    {
                        if (f.Type.Equals("facet", StringComparison.InvariantCultureIgnoreCase))
                        {
                            config.FacetExtractors.Add(new TermFacetExtractor(f.IndexName));
                        }
                        else if (f.Type.Equals("facetpath", StringComparison.InvariantCultureIgnoreCase))
                        {
                            config.FacetExtractors.Add(new TermFacetPathExtractor(f.IndexName));
                        }
                    }
                }
            }

            return config.FacetExtractors.Count > 0 ? config : null;
        }
    }
}
