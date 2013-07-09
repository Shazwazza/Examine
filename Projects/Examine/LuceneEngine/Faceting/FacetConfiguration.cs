using System;
using System.Collections.Generic;
using System.Security;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;

namespace Examine.LuceneEngine.Faceting
{
    public class FacetConfiguration
    {
        private static FacetMap _sharedMap = new FacetMap();


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
            FacetMap = _sharedMap;
            FacetExtractors = new List<IFacetExtractor>();
            FacetCombs = new List<IFacetComb>() { new MaxLevelFacetComb() };
        }
    }

    internal static class FacetConfigurationHelpers
    {
        [SecuritySafeCritical]
        public static FacetConfiguration GetFacetConfiguration(this IndexSet set, FacetConfiguration current = null)
        {
            var config = current ?? new FacetConfiguration();
            foreach (var subset in new[] { set.IndexUserFields, set.IndexAttributeFields })
            {
                if (subset != null)
                {
                    foreach (IIndexField f in subset)
                    {
                        Func<string, IIndexValueType> valueType;
                        if (LuceneIndexer.ConfigurationTypes.TryGetValue(f.Type, out valueType))
                        {
                            var fe = valueType(f.IndexName).CreateFacetExtractor();
                            if (fe != null)
                            {
                                config.FacetExtractors.Add(fe);
                            }
                        }
                    }
                }
            }

            return config;            
        }
    }
}
