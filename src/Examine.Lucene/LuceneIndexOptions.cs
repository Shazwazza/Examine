using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Index;

namespace Examine.Lucene
{

    public class LuceneIndexOptions : IndexOptions
    {
        public bool ReuseDocumentForIndexing { get; set; } = true;

        public bool NrtEnabled { get; set; } = true;

        public double NrtTargetMaxStaleSec { get; set; } = 60.0;

        public double NrtTargetMinStaleSec { get; set; } = 1.0;

        public double NrtCacheMaxMergeSizeMB { get; set; } = 5.0;

        public double NrtCacheMaxCachedMB { get; set; } = 60.0;

        public IndexDeletionPolicy IndexDeletionPolicy { get; set; }

        public Analyzer Analyzer { get; set; }

        /// <summary>
        /// Specifies the index value types to use for this indexer, if this is not specified then the result of <see cref="ValueTypeFactoryCollection.GetDefaultValueTypes"/> will be used.
        /// This is generally used to initialize any custom value types for your indexer since the value type collection cannot be modified at runtime.
        /// </summary>
        public IReadOnlyDictionary<string, IFieldValueTypeFactory> IndexValueTypesFactory { get; set; }
    }
}
