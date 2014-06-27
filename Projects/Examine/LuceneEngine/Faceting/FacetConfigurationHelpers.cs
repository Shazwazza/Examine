using System;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;

namespace Examine.LuceneEngine.Faceting
{
    //NOTE: Facets will not be enabled via configuration

    internal static class FacetConfigurationHelpers
    {

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
                        if (LuceneIndexer.IndexFieldTypes.TryGetValue(f.Type, out valueType))
                        {
                            var fe = valueType(f.Name).CreateFacetExtractor();
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