using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Lucene.Indexing;
using Lucene.Net.Analysis;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Spatial
{
    public class SpatialValueTypeFactoryCollection
    {
        /// <summary>
        /// Returns the default index value types that is used in normal construction of an indexer
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyDictionary<string, IFieldValueTypeFactory> GetDefaultValueTypes(ILoggerFactory loggerFactory, Analyzer defaultAnalyzer)
            => GetDefaults(loggerFactory, defaultAnalyzer).ToDictionary(x => x.Key, x => (IFieldValueTypeFactory)new DelegateFieldValueTypeFactory(x.Value));

        private static IReadOnlyDictionary<string, Func<string, IIndexFieldValueType>> GetDefaults(ILoggerFactory loggerFactory, Analyzer defaultAnalyzer = null)
        {
            return new Dictionary<string, Func<string, IIndexFieldValueType>>(StringComparer.InvariantCultureIgnoreCase) //case insensitive
            {
                {FieldDefinitionTypes.GeoSpatialWKT, name => new WKTSpatialIndexFieldValueType(name, loggerFactory, SpatialIndexFieldValueTypeBase.GeoSpatialPrefixTreeStrategyFactory(),true)},
            };
        }
    }
}
