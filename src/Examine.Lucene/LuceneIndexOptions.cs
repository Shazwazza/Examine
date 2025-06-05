using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Facet;
using Lucene.Net.Index;

namespace Examine.Lucene
{
    /// <summary>
    /// Represents options for a lucene index
    /// </summary>
    public class LuceneIndexOptions : IndexOptions
    {
        /// <summary>  
        /// Gets or sets whether Near Real-Time (NRT) indexing is enabled.  
        /// </summary>  
        public bool NrtEnabled { get; set; } = true;

        /// <summary>  
        /// Gets or sets the maximum stale seconds for Near Real-Time (NRT) indexing.  
        /// This defines the upper limit of staleness for NRT indexing operations.  
        /// </summary>  
        public double NrtTargetMaxStaleSec { get; set; } = 60.0;

        /// <summary>  
        /// Gets or sets the minimum stale seconds for Near Real-Time (NRT) indexing.  
        /// This defines the lower limit of staleness for NRT indexing operations.  
        /// </summary>  
        public double NrtTargetMinStaleSec { get; set; } = 1.0;

        /// <summary>  
        /// Gets or sets the maximum merge size in megabytes for the Near Real-Time (NRT) cache.  
        /// This defines the upper limit of memory usage for merging operations in the NRT cache.  
        /// </summary>  
        public double NrtCacheMaxMergeSizeMB { get; set; } = 5.0;

        /// <summary>  
        /// Gets or sets the maximum cached size in megabytes for the Near Real-Time (NRT) cache.  
        /// This defines the upper limit of memory usage for cached data in the NRT cache.  
        /// </summary>  
        public double NrtCacheMaxCachedMB { get; set; } = 60.0;
        /// <summary>
        /// THe index deletion policy
        /// </summary>
        public IndexDeletionPolicy? IndexDeletionPolicy { get; set; }

        /// <summary>
        /// The analyzer used in the index
        /// </summary>
        public Analyzer? Analyzer { get; set; }

        /// <summary>
        /// Records per-dimension configuration.  By default a
        /// dimension is flat, single valued and does
        /// not require count for the dimension; use
        /// the setters in this class to change these settings for
        /// each dim.
        /// </summary>
        public FacetsConfig FacetsConfig { get; set; } = new FacetsConfig();

        /// <summary>
        /// Specifies the index value types to use for this indexer, if this is not specified then the result of <see cref="ValueTypeFactoryCollection.GetDefaultValueTypes"/> will be used.
        /// This is generally used to initialize any custom value types for your indexer since the value type collection cannot be modified at runtime.
        /// </summary>
        public IReadOnlyDictionary<string, IFieldValueTypeFactory>? IndexValueTypesFactory { get; set; }
    }
}
