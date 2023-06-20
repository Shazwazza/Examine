using System;
using System.Collections.Generic;
using System.Text;
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
        /// THe index deletion policy
        /// </summary>
        public IndexDeletionPolicy IndexDeletionPolicy { get; set; }

        /// <summary>
        /// The analyzer used in the index
        /// </summary>
        public Analyzer Analyzer { get; set; }

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
        public IReadOnlyDictionary<string, IFieldValueTypeFactory> IndexValueTypesFactory { get; set; }
    }
}
