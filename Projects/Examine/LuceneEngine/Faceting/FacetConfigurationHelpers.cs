using System;
using System.Linq;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;

namespace Examine.LuceneEngine.Faceting
{
    /// <summary>
    /// This is used to try and get facet configuration enabled for indexes that are using the old config.
    /// </summary>
    internal static class FacetConfigurationHelpers
    {
        /// <summary>
        /// This is used to try and get facet configuration enabled for indexes that are using the old config.
        /// </summary>
        /// <param name="set"></param>
        /// <param name="indexer"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public static FacetConfiguration GetFacetConfiguration(this IndexSet set, LuceneIndexer indexer, FacetConfiguration current = null)
        {
            if (set == null) throw new ArgumentNullException("set");
            if (indexer == null) throw new ArgumentNullException("indexer");

            var config = current ?? new FacetConfiguration();
            foreach (var subset in new[] { set.IndexUserFields, set.IndexAttributeFields })
            {
                if (subset != null)
                {
                    foreach (IIndexField f in subset)
                    {
                        Func<string, IIndexValueType> valueType;
                        if (indexer.IndexFieldTypes.TryGetValue(f.Type, out valueType))
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